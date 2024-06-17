using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel;
using System.Collections.Generic;

namespace BourceBlazor.Services;

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

        foreach (var formol in formols)
        {
            Restart = true;
            MainRealBaseTradeHistories.Clear();
            DeletedTradeHistories.Clear();

            while (Restart)
            {
                BaseTradeHistories.Clear();

                if (TradeHistoriesList.Count <= CurrentMultiStage)
                {
                    //نتیجه نهایی
                    ResultEnd = SetEndResult();
                    TradeHistoriesList = ResultEnd.MainRealBaseTradeHistories;
                    continue;
                }

                //============تنظیم ردیف های پایه=====================//

                SetBaseTradeHistories(TradeHistoriesList);

                //=========== تنظیم مقادیر اصلی ردیف پایه=============//

                var result = SetBaseTradeHistoriesViewModel(formol);

                if (!result)
                {
                    return new ResultCalculateFormol()
                    {
                        ErrorMessage = "firstTradeHistories is null"
                    };
                }

                //=========== محاسبه خود ردیف پایه با فرمول===========//

                var resultBaseTradeWithFormol = SetBaseTradeHistoriesViewModelWithFormol(formol, TradeHistoriesList);

                if (resultBaseTradeWithFormol)
                {
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

                if (counTradeHistoriesListt == 0 || TradeHistoriesList.Count <= CurrentMultiStage)
                {
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

    /// <summary>
    /// نتیجه نهایی
    /// </summary>
    /// <param name="TradeHistoriesList"></param>
    /// <returns></returns>
    private ResultCalculateFormol SetEndResult()
    {
        SumHajm = MainRealBaseTradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = MainRealBaseTradeHistories.Select(x => x.nTran).Count().ToString("#,0");
        Restart = false;

        var model = new ResultCalculateFormol()
        {
            DeletedTradeHistories = DeletedTradeHistories.OrderBy(x => x.nTran).ToList(),
            MainRealBaseTradeHistories = MainRealBaseTradeHistories.OrderBy(x => x.nTran).ToList(),
            SumCount = SumCount,
            SumHajm = SumHajm,
            ErrorMessage = "successful",
            IsSuccess = true
        };

        CurrentMultiStage = 1;

        return model;
    }

    /// <summary>
    /// تنظیم ردیف های پایه
    /// </summary>
    /// <param name="TradeHistoriesList"></param>
    private void SetBaseTradeHistories(List<TradeHistory> TradeHistoriesList)
    {
        var result = TradeHistoriesList
                      .Skip(0)
                      .Take(CurrentMultiStage)
                      .ToList();

        BaseTradeHistories.AddRange(result);
    }

    /// <summary>
    ///  تنظیم مقادیر اصلی ردیف پایه
    /// </summary>
    /// <param name="formol"></param>
    /// <returns></returns>
    private bool SetBaseTradeHistoriesViewModel(FormolSendAction formol)
    {
        if (CurrentMultiStage == 1)
        {
            var firstTradeHistories = BaseTradeHistories.FirstOrDefault();

            if (firstTradeHistories == null)
            {
                return false;
            }

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseHajm = firstTradeHistories.qTitTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.TimeFormol;
        }
        else if (CurrentMultiStage > 1)
        {
            var firstTradeHistories = BaseTradeHistories.Skip(0).Take(1).FirstOrDefault();

            if (firstTradeHistories == null)
            {
                return false;
            }

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.TimeFormol;
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
        if (BaseTradeHistoriesViewModel.BaseHajm == formol.HajmFormol)
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
        var calculateTradeHistories = TradeHistoriesList
                                          .Skip(CurrentMultiStage)
                                          .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                          .ToList();

        var validation = calculateTradeHistories.Count < BaseTradeHistories.Count ||
                         (calculateTradeHistories.Count == 0 && BaseTradeHistories.Count != 0) ||
                         calculateTradeHistories.Count == BaseTradeHistories.Count;

        if (validation)
        {
            if (calculateTradeHistories.Count == BaseTradeHistories.Count)
            {
                //در صورتی که آخرین ردیف باشد و پایه با کل معاملات تعداشون برابر باشد
                var firstcalculateTradeHistories = calculateTradeHistories.FirstOrDefault();

                if (firstcalculateTradeHistories != null)
                {
                    TradeHistoriesList.Remove(firstcalculateTradeHistories);
                    MainRealBaseTradeHistories.Add(firstcalculateTradeHistories);
                    CurrentMultiStage = 1;
                }
            }

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

        foreach (var item in calculateTradeHistories)
        {
            if (!isFinshFor)
            {
                var sum = BaseTradeHistoriesViewModel.BaseHajm + item.qTitTran;

                if (sum == formol.HajmFormol)
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

        if (isFinshFor)
        {
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

    #endregion
}
