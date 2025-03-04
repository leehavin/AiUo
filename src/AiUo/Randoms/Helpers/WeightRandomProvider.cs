using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUo.Randoms;

public class WeightRandomProvider<T>
{
    private List<WeightRandomItem<T>> _list = new();
    private bool _init;
    private object _sync = new object();
    private int _totalWeight;
    private List<WeightRandomItem<T>> _calcList;
    public void AddItem(int weight, T item)
    {
        _init = false;
        _list.Add(new WeightRandomItem<T>
        {
            Weight = weight,
            Item = item
        });
    }
    public void ClearItems()
    {
        _init = false;
        _list.Clear();
    }
    public void Init()
    {
        if (!_init)
        {
            lock(_sync)
            {
                if (!_init)
                {
                    _calcList = _list.Where(x => x.Weight > 0)
                        .OrderBy(x => x.Weight).ToList();
                    _totalWeight = _calcList.Sum(x => x.Weight);
                    _init = true;
                }
            }
        }
    }
    public T Next()
    {
        Init();
        int cursor = 0;
        int rnd = RandomUtil.NextInt(0, _totalWeight);
        foreach (var item in _calcList)
        {
            cursor += item.Weight;
            if (cursor > rnd)
            {
                return item.Item;
            }
        }
        throw new Exception("WeightRandomHelper没有获得随机数!");
    }
    class WeightRandomItem<TItem>
    {
        public int Weight { get; set; }
        public TItem Item { get; set; }
    }
}