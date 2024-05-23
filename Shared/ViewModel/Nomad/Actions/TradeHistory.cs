namespace AppShared.ViewModel.Nomad.Actions
{

    public class Root
    {
        public List<TradeHistory> tradeHistory { get; set; }
    }

    public class TradeHistory
    {
        public object insCode { get; set; }
        public int dEven { get; set; }
        public int nTran { get; set; }
        public int hEven { get; set; }
        public int qTitTran { get; set; }
        public double pTran { get; set; }
        public int qTitNgJ { get; set; }
        public string iSensVarP { get; set; }
        public double pPhSeaCotJ { get; set; }
        public double pPbSeaCotJ { get; set; }
        public int iAnuTran { get; set; }
        public double xqVarPJDrPRf { get; set; }
        public int canceled { get; set; }
    }
}
