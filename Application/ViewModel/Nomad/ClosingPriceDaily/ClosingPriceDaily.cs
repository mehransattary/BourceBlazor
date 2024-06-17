namespace Application.ViewModel.Nomad.ClosingPriceDaily;

public class ClosingPriceDaily
{
    public int Counter { get; set; }
    public int id { get; set; }
    public string insCode { get; set; }
    /// <summary>
    /// تاریخ
    /// </summary>
    public int dEven { get; set; }

    /// <summary>
    /// تاریخ فارسی
    /// </summary>
    public string dEvenPersian { get; set; }

    /// <summary>
    /// تعداد
    /// </summary>
    public double zTotTran { get; set; }
    /// <summary>
    /// حجم
    /// </summary>
    public double qTotTran5J { get; set; }
    /// <summary>
    /// ارزش
    /// </summary>
    public double qTotCap { get; set; }
    public double priceChange { get; set; }
    public double priceMin { get; set; }
    public double priceMax { get; set; }
    public double priceYesterday { get; set; }
    public double priceFirst { get; set; }
    public bool last { get; set; }
    public int hEven { get; set; }
    public double pClosing { get; set; }
    public bool iClose { get; set; }
    public bool yClose { get; set; }
    public double pDrCotVal { get; set; }

}
