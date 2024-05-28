namespace AppShared.Entities;

public class NomadAction : BourceBase
{
    /// <summary>
    /// //ردیف
    /// </summary>
    public long NTran { get; set; }

    /// <summary>
    /// زمان
    /// </summary>
    public TimeOnly HEven { get; set; }

    /// <summary>
    /// حجم
    /// </summary>
    public long QTitTran { get; set; }

    /// <summary>
    /// قیمت
    /// </summary>
    public decimal PTran { get; set; }

    /// <summary>
    /// غیر فعال کردن
    /// </summary>
    public bool IsDisable { get; set; }

    /// <summary>
    /// نماد
    /// </summary>
    public int NomadDateId { get; set; }

    public NomadDate? NomadDate { get; set; }
}

