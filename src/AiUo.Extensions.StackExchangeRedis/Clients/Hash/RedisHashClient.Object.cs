using StackExchange.Redis;
using System.Linq.Expressions;
using AiUo.Caching;
using AiUo.Reflection;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis Hash表（key-value结构） value值可以是任意object类型
///     可以被继承，也可以直接构建
///     RedisKey => Field => RedisValue
///     可存入null值
/// </summary>
public class RedisHashClient : RedisHashBase<object>
{
    #region Common
    /// <summary>
    /// 【创建或更新】设置hash结构中的field对应的缓存值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="always">true:无论是否存在总是添加，false：不存在时才添加</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<T>(string field, T value, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync<T>(field, value, always, flags);
    public async Task<CacheValue<T>> GetAsync<T>(string field, CommandFlags flags = CommandFlags.None)
        => await GetBaseAsync<T>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在则抛出异常CacheNotFound
    /// </summary>
    /// <param name="field"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrExceptionAsync<T>(string field, CommandFlags flags = CommandFlags.None)
        => await GetOrExceptionBaseAsync<T>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在，则返回默认值。
    /// </summary>
    /// <param name="field"></param>
    /// <param name="defaultValue"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrDefaultAsync<T>(string field, T defaultValue, CommandFlags flags = CommandFlags.None)
        => await GetOrDefaultBaseAsync(field, defaultValue, flags);
    #endregion

    #region Entity
    /// <summary>
    /// 【创建或更新】根据item对象的属性名和值设置hash结构中的field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="flags"></param>
    public async Task SetEntityAsync<T>(T item, CommandFlags flags = CommandFlags.None)
        where T : new()
    {
        var entries = GetHashEntries(item);
        await Database.HashSetAsync(RedisKey, entries, flags);
        await SetSlidingExpirationAsync();
    }
    public async Task<T> GetEntityAsync<T>(CommandFlags flags = CommandFlags.None)
        where T : new()
    {
        var entries = new List<HashEntry>();
        var prefix = typeof(T).FullName;
        await foreach (var entry in Database.HashScanAsync(RedisKey, $"{prefix}|*",flags: flags))
        {
            entries.Add(entry);
        }
        var ret = GetEntityByHashEntry<T>(entries);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 根据设置的实体值保存
    /// demo: SetEntityAsync(() => new UserInfo {UserId = 2, Name = "Test2"});
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="expr"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task SetEntityAsync<TEntity>(Expression<Func<TEntity>> expr, CommandFlags flags = CommandFlags.None)
    {
        if (expr.Body.NodeType != ExpressionType.MemberInit)
            throw new Exception("RedisHashClient.SetEntityAsync()暂时不支持除ExpressionType.MemberInit类型外的成员绑定表达式");

        var entries = new List<HashEntry>();
        var memberInitExpr = expr.Body as MemberInitExpression;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            var elementExpr = memberInitExpr.Bindings[i];
            if (elementExpr.BindingType != MemberBindingType.Assignment)
                throw new Exception("RedisHashClient.SetEntityAsync()暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
            if (elementExpr is MemberAssignment memberAssignment)
            {
                var value = this.Evaluate(memberAssignment.Expression);
                var field = memberAssignment.Member.Name;
                var name = $"{typeof(TEntity).FullName}|{field}";
                entries.Add(new HashEntry(name, Serialize(value)));
            }
        }
        await Database.HashSetAsync(RedisKey, entries.ToArray(), flags);
        await SetSlidingExpirationAsync();
    }
    /// <summary>
    /// 根据指定的属性获取值
    /// demo: GetEntityAsync(it => new {it.UserId,it.Name });
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="fieldsExpr"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<TEntity> GetEntityAsync<TEntity>(Expression<Func<TEntity, object>> fieldsExpr, CommandFlags flags = CommandFlags.None)
        where TEntity : class, new()
    {
        var fields = new List<RedisValue>();
        switch (fieldsExpr.Body)
        {
            case NewExpression newExpr:
                for (var i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var item = newExpr.Arguments[i];
                    if (item is MemberExpression newMemExpr)
                    {
                        var field = newMemExpr.Member.Name;
                        fields.Add($"{typeof(TEntity).FullName}|{field}");
                    }
                }
                break;
            case MemberExpression memExpr:
                fields.Add($"{typeof(TEntity).FullName}|{memExpr.Member.Name}");
                break;
            case UnaryExpression oneExpr:
                if (!(oneExpr.Operand is MemberExpression memExpr2))
                    throw new Exception("RedisHashClient.SetEntityAsync()仅支持UnaryExpression和MemberExpression");
                fields.Add($"{typeof(TEntity).FullName}|{memExpr2.Member.Name}");
                break;
        }
        var values = await Database.HashGetAsync(RedisKey, fields.ToArray(), flags);
        var entries = new List<HashEntry>();
        for (int i = 0; i < values.Length; i++)
        {
            entries.Add(new HashEntry(fields[i], values[i]));
        }
        var ret = GetEntityByHashEntry<TEntity>(entries);
        await SetSlidingExpirationAsync();
        return ret;
    }
    private object Evaluate(Expression expr)
    {
        var lambdaExpr = Expression.Lambda(expr);
        return lambdaExpr.Compile().DynamicInvoke();
    }

    private HashEntry[] GetHashEntries<T>(T item)
        where T : new()
    {
        var ret = new List<HashEntry>();
        var props = typeof(T).GetProperties();
        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            if (prop.IsDefined(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), true)
                || prop.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), true))
                continue;
            var propValue = ReflectionUtil.GetPropertyValue(item, prop.Name);
            var redisValue = Serialize(propValue);
            var field = $"{prop.DeclaringType.FullName}|{prop.Name}";
            ret.Add(new HashEntry(field, redisValue));
        }
        return ret.ToArray();
    }

    private T GetEntityByHashEntry<T>(List<HashEntry> entries)
        where T : new()
    {
        if (entries == null || entries.Count == 0)
            return default(T);
        T ret = new T();
        var props = ReflectionUtil.GetPropertyDic<T>();
        foreach (var entry in entries)
        {
            var propName = Convert.ToString(entry.Name).Split('|')[1];
            if (!props.ContainsKey(propName))
                continue;
            if (!TryDeserialize(entry.Value, props[propName].PropertyType, out object propValue))
                continue;
            ReflectionUtil.SetPropertyValue(ret, propName, propValue);
        }
        return ret;
    }
    #endregion

    #region GetOrLoad
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在则调用LoadValueWhenRedisNotExists()放入redis并返回
    /// </summary>
    /// <param name="field"></param>
    /// <param name="enforce">是否强制Load</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<T>> GetOrLoadAsync<T>(string field, bool enforce = false, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<T> ret;
        if (enforce || !TryDeserialize(await Database.HashGetAsync(RedisKey, field, flags), out T value))
        {
            var loadValue = await LoadValueWhenRedisNotExistsAsync(field);
            if (loadValue.HasValue)
            {
                await Database.HashSetAsync(RedisKey, field, Serialize(loadValue.Value), When.Always, flags);
                ret = new CacheValue<T>(true, (T)loadValue.Value);
            }
            else
            {
                if (IsLoadValueNotExistsToRedis)
                    await Database.HashSetAsync(RedisKey, field, Serialize(null), When.Always, flags);
                ret = new CacheValue<T>(false);
            }
        }
        else
        {
            ret = new CacheValue<T>(true, value);
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}