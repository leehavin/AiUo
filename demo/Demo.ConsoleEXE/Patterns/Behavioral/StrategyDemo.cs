namespace AiUo.Demos.Patterns.Behavioral;

/// <summary>
/// 策略模式
/// </summary>
internal class StrategyDemo : DemoBase
{
    public override async Task Execute()
    {
        // 个人所得税方式
        InterestOperation operation = new InterestOperation(new PersonalTaxStrategy());
        Console.WriteLine("个人支付的税为：{0}", operation.GetTax(5000.00));

        // 企业所得税
        operation = new InterestOperation(new EnterpriseTaxStrategy());
        Console.WriteLine("企业支付的税为：{0}", operation.GetTax(50000.00));
    }
}
// 所得税计算策略
public interface ITaxStragety
{
    double CalculateTax(double income);
}

// 个人所得税
public class PersonalTaxStrategy : ITaxStragety
{
    public double CalculateTax(double income)
    {
        return income * 0.12;
    }
}

// 企业所得税
public class EnterpriseTaxStrategy : ITaxStragety
{
    public double CalculateTax(double income)
    {
        return (income - 3500) > 0 ? (income - 3500) * 0.045 : 0.0;
    }
}

public class InterestOperation
{
    private ITaxStragety m_strategy;
    public InterestOperation(ITaxStragety strategy)
    {
        this.m_strategy = strategy;
    }

    public double GetTax(double income)
    {
        return m_strategy.CalculateTax(income);
    }
}