using Application.ViewModel;
using Application.ViewModel.Nomad.Actions;
using System.Diagnostics.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services;

public class FormolService : IFormolService
{
    #region Fields

    //ردیف های حذف شده
    private List<TradeHistory> DeletedTradeHistories = new();

    //ردیف های واقعی
    private List<TradeHistory> MainRealBaseTradeHistories = new();

    //ردیف های پایه
    private List<TradeHistory> BaseTradeHistories = new();

    //مقادیر  ردیف پایه
    private BaseTradeHistoriesViewModel BaseTradeHistoriesViewModel { get; set; } = new();

    //مرحله جاری
    private int CurrentMultiStage = 1;

    //اتمام مرحله
    private bool Restart = true;

    //جمع کل حجم
    private string SumHajm { get; set; } = string.Empty;

    //تعداد کل معاملات
    private string SumCount { get; set; } = string.Empty;

    public long Counterx { get; set; } = 1;
    #endregion

    #region Main Methods

    /// <summary>
    /// محاسبه فرمول اصلی
    /// </summary>
    /// <param name="formol"></param>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    public async Task<ResultCalculateFormol> GetFilterByFormolAll(List<FormolSendAction> formols,
                                                                  List<TradeHistory> TradeHistoriesList)
    {
        ResultCalculateFormol ResultEnd = new();

        MainRealBaseTradeHistories.Clear();
        DeletedTradeHistories.Clear();

        foreach (var formol in formols)
        {
            Restart = true;

            while (Restart)
            {
                Counterx++;

                BaseTradeHistories.Clear();

                if (TradeHistoriesList.Count <= CurrentMultiStage)
                {
                    AddMainRealBaseTradeHistories(TradeHistoriesList);

                    //نتیجه نهایی
                    ResultEnd = SetEndResult();
                    TradeHistoriesList = ResultEnd.MainRealBaseTradeHistories;
                    continue;
                }

                //========== محاسبه ردیف های پایه با دیگر ردیف ها=====//

                var resultCalculateTradeHistories = GetCalculateTradeHistories(formol, TradeHistoriesList);

                if (resultCalculateTradeHistories)
                {
                    continue;
                }

                //=========تنظیم مراحل ومقایسه آنها====================//

                var resultComparison = ComparisonCurrentMultiStageWithMultiStage(formol, TradeHistoriesList);

                if (resultComparison)
                {
                    continue;
                }

                var counTradeHistoriesListt = TradeHistoriesList.Count;

                if (counTradeHistoriesListt == (0) || TradeHistoriesList.Count <= CurrentMultiStage)
                {
                    AddMainRealBaseTradeHistories(TradeHistoriesList);
                    ResultEnd = SetEndResult();
                    TradeHistoriesList = ResultEnd.MainRealBaseTradeHistories;
                    continue;
                }
            }
        }

        return ResultEnd;
    }



    #endregion

    #region Private Methods

    private void AddMainRealBaseTradeHistories(List<TradeHistory> TradeHistoriesList)
    {
        if (TradeHistoriesList.Count <= CurrentMultiStage)
        {
            TradeHistoriesList.ForEach(item =>
            {
                MainRealBaseTradeHistories.Add(item);
            });
        }
    }

    /// <summary>
    /// نتیجه نهایی
    /// </summary>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    private ResultCalculateFormol SetEndResult()
    {
        var resMainRealBaseTradeHistories = MainRealBaseTradeHistories.Distinct().Except(DeletedTradeHistories.Distinct()).ToList();
        SumHajm = resMainRealBaseTradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = resMainRealBaseTradeHistories.Select(x => x.nTran).Count().ToString("#,0");
        Restart = false;

        var model = new ResultCalculateFormol()
        {
            DeletedTradeHistories = DeletedTradeHistories.ToList(),
            MainRealBaseTradeHistories = resMainRealBaseTradeHistories.OrderBy(x => x.nTran).ToList(),
            SumCount = SumCount,
            SumHajm = SumHajm,
            ErrorMessage = "successful",
            IsSuccess = true
        };

        CurrentMultiStage = 1;

        return model;
    }

    /// <summary>
    ///  تنظیم مقادیر اصلی ردیف پایه اصلی
    /// </summary>
    /// <param name="formol"></param>
    /// <returns></returns>
    private bool SetBaseTradeHistoriesViewModel(FormolSendAction formol)
    {
        if (CurrentMultiStage == (1))
        {
            var firstTradeHistories = BaseTradeHistories.FirstOrDefault();

            if (firstTradeHistories == null)
            {
                return false;
            }

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseHajm = firstTradeHistories.qTitTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = ConvertTime(firstTradeHistories.hEven, formol.TimeFormol);
        }
        else if (CurrentMultiStage > 1)
        {
            var firstTradeHistories = BaseTradeHistories.Skip(0).Take(1).FirstOrDefault();

            if (firstTradeHistories is null)
            {
                return false;
            }

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = ConvertTime(firstTradeHistories.hEven, formol.TimeFormol);
            BaseTradeHistoriesViewModel.BaseHajm = BaseTradeHistories.Sum(b => b.qTitTran);
        }

        return true;
    }

    /// <summary>
    /// محاسبه خود ردیف پایه با فرمول
    /// </summary>
    /// <param name="formol"></param>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    private bool SetBaseTradeHistoriesViewModelWithFormol(FormolSendAction formol, List<TradeHistory> TradeHistoriesList)
    {
        if (BaseTradeHistoriesViewModel.BaseHajm == (formol.HajmFormol))
        {
            BaseTradeHistories.ForEach(item =>
            {
                TradeHistoriesList.Remove(item);
                DeletedTradeHistories.Add(item);
            });

            CurrentMultiStage = 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// تنظیم مراحل ومقایسه آنها
    /// </summary>
    /// <param name="formol"></param>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    private bool GetCalculateTradeHistories(FormolSendAction formol, List<TradeHistory> TradeHistoriesList)
    {
        var firstTrade = TradeHistoriesList.OrderBy(x => x.nTran).FirstOrDefault();
        BaseTradeHistories.Add(firstTrade);
        SetBaseTradeHistoriesViewModel(formol);

        //ردیف هایی که باید محاسبه شوند طبق شرایط زمان ومراحل
        var calculateTradeHistories = TradeHistoriesList
                                          .Skip(1)
                                          .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                          .ToList();

        if (calculateTradeHistories.Count == 0)
        {
            //اولین ردیف از پایه هارا حذف کن
            var first = BaseTradeHistories.FirstOrDefault();

            if (first != null)
            {
                TradeHistoriesList.Remove(first);
                MainRealBaseTradeHistories.Add(first);
                CurrentMultiStage = 1;
            }

            return true;
        }

        bool isFinshFor = false;

        int countCalculateTradeHistories = calculateTradeHistories.Count;

        int counter =1;

        do
        {
            if (counter > 1 && BaseTradeHistories.Count > 1)
            {
                var lastRecord = BaseTradeHistories.Skip(1).Take(1).FirstOrDefault();
                calculateTradeHistories.Remove(lastRecord);
                BaseTradeHistories.Remove(lastRecord);
                BaseTradeHistories.ForEach(item => calculateTradeHistories.Remove(item));
            }

            if (!calculateTradeHistories.Any())
            {
                return false;
            }

            foreach (var item in calculateTradeHistories)
            {
                if (!isFinshFor)
                {
                    if (BaseTradeHistories.Count < CurrentMultiStage)
                    {
                        BaseTradeHistories.Add(item);
                        SetBaseTradeHistoriesViewModel(formol);
                        var result = SetBaseTradeHistoriesViewModelWithFormol(formol, TradeHistoriesList);
                        if (result)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        var sum = BaseTradeHistoriesViewModel.BaseHajm + item.qTitTran;

                        var validation = formol.HajmFormol == sum;

                        if (validation)
                        {
                            BaseTradeHistories.ForEach(b =>
                            {
                                TradeHistoriesList.Remove(b);
                                DeletedTradeHistories.Add(b);
                            });

                            TradeHistoriesList.Remove(item);
                            DeletedTradeHistories.Add(item);
                            CurrentMultiStage = 1;
                            isFinshFor = true;
                        }
                    }

                }

            }

            counter += 1;

            if (isFinshFor)
            {
                return true;
            }

        } while (counter <= countCalculateTradeHistories && CurrentMultiStage > 1);

    

        return false;
    }

    /// <summary>
    /// تنظیم مراحل ومقایسه آنها
    /// </summary>
    /// <param name="formol"></param>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    private bool ComparisonCurrentMultiStageWithMultiStage(FormolSendAction formol, List<TradeHistory> TradeHistoriesList)
    {
        //مرحله جاری را افزایش بده
        CurrentMultiStage = CurrentMultiStage + 1;

        var multiStage = formol.MultiStage;

        // در صورتی که به مرحله آخر رسیده باشید
        if (multiStage < CurrentMultiStage)
        {
            var first = BaseTradeHistories.FirstOrDefault();

            if (first != null)
            {
                TradeHistoriesList.Remove(first);
                MainRealBaseTradeHistories.Add(first);
                CurrentMultiStage = 1;
                return true;
            }
        }

        return false;
    }

    private int ConvertTime(int time, int formolTime)
    {
        //90021
        var _string = time.ToString();
        var _secondTime = _string.Substring(_string.Length - 2);
        var _minutesTime = _string.Substring(_string.Length - 4).Substring(0, 2);
        string _hoursTime = string.Empty;

        if (_string.Length == 5)
        {
            _hoursTime = _string.Substring(_string.Length - 5).Substring(0, 1);
        }
        else
        {
            _hoursTime = _string.Substring(_string.Length - 6).Substring(0, 2);
        }

        int secondTime = int.Parse(_secondTime);
        int minutesTime = int.Parse(_minutesTime);
        int hoursTime = int.Parse(_hoursTime);

        int resSecond = 0;
        string ResultEnd = string.Empty;


        var validSecond = (secondTime + formolTime) < 60;

        if (validSecond)
        {
            resSecond = secondTime + formolTime;
            var updateResSecond = ConvertNumberTwoLength(resSecond);
            var updateResMin = ConvertNumberTwoLength(minutesTime);

            ResultEnd = hoursTime + updateResMin + updateResSecond;
        }
        else
        {
            resSecond = secondTime + formolTime;
            var second = resSecond % 60;
            var minutes = resSecond / 60;

            var validMinutes = (minutesTime + minutes) < 60;

            if (validMinutes)
            {
                var updateResMin = ConvertNumberTwoLength((minutesTime + minutes));
                var updateRessecond = ConvertNumberTwoLength(second);

                ResultEnd = hoursTime + updateResMin + updateRessecond;
            }
            else
            {
                var minites = (minutesTime + minutes) % 60;
                var hours = (minutesTime + minutes) / 60;
                var updateResminites = ConvertNumberTwoLength(minites);
                var updateRessecond = ConvertNumberTwoLength(second);

                ResultEnd = (hoursTime + hours) + updateResminites + updateRessecond;
            }
        }

        return int.Parse(ResultEnd);
    }

    private string ConvertNumberTwoLength(int number)
    {
        var res = number.ToString().Length < 2 ? "0" + number : number.ToString();

        return res;
    }
    #endregion
}
