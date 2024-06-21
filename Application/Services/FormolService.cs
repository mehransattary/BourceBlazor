﻿using Application.ViewModel;
using Application.ViewModel.Nomad.Actions;
using System.Diagnostics.Metrics;

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

                //============تنظیم ردیف های پایه=====================//

                SetBaseTradeHistories(formol, TradeHistoriesList);

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
    /// تنظیم ردیف های پایه
    /// </summary>
    /// <param name="TradeHistoriesList"></param>
    private void SetBaseTradeHistories(FormolSendAction formol, List<TradeHistory> TradeHistoriesList)
    {
        List<TradeHistory> addBaseTradeHistories = [];

        if (CurrentMultiStage > 1)
        {
            var firstBase = TradeHistoriesList.OrderBy(x => x.nTran)
                                              .FirstOrDefault();

            if (firstBase is null)
            {
                return;
            }

            //اگر  قیمت ردیف پایه با قیمت معاملاتی که با این قیمت برابر باشد به ترتیب جمع کن
            addBaseTradeHistories = TradeHistoriesList.Where(x => !formol.CalculationPrice || x.pTran == (firstBase.pTran))
                                                      .Skip(0)
                                                      .Take(CurrentMultiStage)
                                                      .ToList();
        }
        else
        {
            addBaseTradeHistories = TradeHistoriesList
                                    .Skip(0)
                                    .Take(CurrentMultiStage)
                                    .ToList();
        }

        BaseTradeHistories.AddRange(addBaseTradeHistories);

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
            BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.TimeFormol;
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

        var lastBaseTrade = BaseTradeHistories.OrderByDescending(x => x.nTran).FirstOrDefault();

        //ردیف هایی که باید محاسبه شوند طبق شرایط زمان ومراحل
        var calculateTradeHistories = TradeHistoriesList
                                          //.Where(x=> !(lastBaseTrade!=null) || x.nTran >= lastBaseTrade.nTran)
                                          .Skip(CurrentMultiStage)
                                          .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                          .ToList();

        // اگر تعدا ردیف های پایه بزرگتر از تعداد  معاملاتی که قرار است محاسبه شوند
        //اگر ردیف های پایه مخالف صفر باشند و تعداد معاملات محاسباتی هم صفر باشد 
        //اگر تعداد معاملات محاسبه شده برابر ردیف های پایه باشد 
        //var validation = calculateTradeHistories.Count < BaseTradeHistories.Count ||
        //                 calculateTradeHistories.Count == (0) && BaseTradeHistories.Count != 0 ||
        //                 calculateTradeHistories.Count == BaseTradeHistories.Count;
        //تعداد معاملات برابر صفر شود
        //calculateTradeHistories.Count == 0
     
        if (calculateTradeHistories.Count == 0 )
        {
            //if (calculateTradeHistories.Count == BaseTradeHistories.Count)
            //{
            //    //در صورتی که آخرین ردیف باشد و پایه با کل معاملات تعداشون برابر باشد
            //    var firstcalculateTradeHistories = calculateTradeHistories.FirstOrDefault();

            //    if (firstcalculateTradeHistories != null)
            //    {
            //        TradeHistoriesList.Remove(firstcalculateTradeHistories);
            //        MainRealBaseTradeHistories.Add(firstcalculateTradeHistories);
            //        CurrentMultiStage = 1;
            //    }
            //}

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

                //محاسبه بر اساس قیمت
                if (formol.CalculationPrice)
                {
                    //.اگر قیمت ردیف پایه با قیمت ردیف های دیگر برابر بود مقایسه انجام شود
                    if (item.pTran == (BaseTradeHistoriesViewModel.BasePrice))
                    {
                        getCalculate();
                    }
                }
                //محاسبه بر اساس بدون قیمت
                else
                {
                    getCalculate();
                }

                void getCalculate()
                {
                    var validation = formol.HajmFormol == (sum);

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
        CurrentMultiStage = CurrentMultiStage + 1 ;

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
