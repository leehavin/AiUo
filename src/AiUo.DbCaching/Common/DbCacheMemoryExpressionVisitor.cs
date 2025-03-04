using System.Linq.Expressions;

namespace AiUo.DbCaching;

internal class DbCacheMemoryExpressionVisitor : ExpressionVisitor
{
    private List<string> DictKeys = new();
    private List<string> ValueKeys = new();

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        DictKeys.Add(node.Member.Name);
        return base.VisitMemberAssignment(node);
    }

    //protected override Expression VisitConstant(ConstantExpression node)
    //{
    //    //if (ReflectionUtil.IsSimpleType(node.Type))
    //    //    ValueKeys.Add(Convert.ToString(node.Value));

    //    if (!node.Type.IsEntityType(out _))
    //    {
    //        if (node.Value is null) ValueKeys.Add("");
    //        else ValueKeys.Add(node.Value.ToString());
    //    }
    //    return base.VisitConstant(node);
    //}
    protected override Expression VisitMember(MemberExpression node)
    {
        ValueKeys.Add(node.Member.Name);
        var memberValue = Expression.Lambda(node).Compile().DynamicInvoke();
        ValueKeys.Add(memberValue.ToString());
        //var value = GetMemberValue(node);
        //ValueKeys.Add(Convert.ToString(value));
        return base.VisitMember(node);
    }
    //private static object GetMemberValue(MemberExpression expression)
    //{
    //    if (expression == null)
    //        return null;
    //    var field = expression.Member as FieldInfo;
    //    if (field != null)
    //    {
    //        var constValue = GetConstantValue(expression.Expression);
    //        return field.GetValue(constValue);
    //    }
    //    var property = expression.Member as PropertyInfo;
    //    if (property == null)
    //        return null;
    //    var value = GetMemberValue(expression.Expression as MemberExpression);
    //    return property.GetValue(value);
    //}

    //private static object GetConstantValue(Expression expression)
    //{
    //    var constantExpression = expression as ConstantExpression;
    //    if (constantExpression == null)
    //        return null;
    //    return constantExpression.Value;
    //}
    public (string DictKey, string ValueKey) GetKeys()
    {
        return (string.Join('|', DictKeys), string.Join('|', ValueKeys));
    }
}