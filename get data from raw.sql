SELECT TOP (1000)
    [Id],
    [UserGuid],
    [CalculatorType],
    [Data],
	--LastUpdateDate,
    jsonData.CurrentAge,
    jsonData.RetirementAge,
    jsonData.InitialTaxableAmount,
    jsonData.InitialTraditionalAmount,
    jsonData.InitialRothAmount,
    jsonData.MonthlyTaxableContribution,
    jsonData.MonthlyTraditionalContribution,
    jsonData.MonthlyRothContribution,
    jsonData.AnnualGrowthRate,
    jsonData.Years,
	jsonData.RetirementAge
FROM [dev-glamourre].[finplan].[FinPlan]
CROSS APPLY OPENJSON([Data])
WITH (
    CurrentAge INT,
    RetirementAge INT,
    InitialTaxableAmount DECIMAL(18,2),
    InitialTraditionalAmount DECIMAL(18,2),
    InitialRothAmount DECIMAL(18,2),
    MonthlyTaxableContribution DECIMAL(18,2),
    MonthlyTraditionalContribution DECIMAL(18,2),
    MonthlyRothContribution DECIMAL(18,2),
    AnnualGrowthRate DECIMAL(5,2),
    Years INT,
	RetirementAge int
--	LastUpdateDate DateTime
) AS jsonData
where CalculatorType like '%sav%'

  --truncate table [dev-glamourre].[finplan].[FinPlan]
  SELECT TOP (1000)
    [Id],
    [UserGuid],
    [CalculatorType],
    [Data],
	--LastUpdateDate,
    jsonData.RetirementAge,
	jsonData.LifeExpectancy,
	jsonData.PlanYears,
    jsonData.TaxableBalance,
    jsonData.TraditionalBalance,
    jsonData.RothBalance,
    jsonData.MonthlyTaxableContribution,
    jsonData.MonthlyTraditionalContribution,
    jsonData.MonthlyRothContribution,
    jsonData.AnnualGrowthRate,
    jsonData.Years,
	jsonData.RetirementAge
FROM [dev-glamourre].[finplan].[FinPlan]
CROSS APPLY OPENJSON([Data])
WITH (
    RetirementAge INT,
	LifeExpectancy INT,
	PlanYears INT,
    TaxableBalance DECIMAL(18,2),
    TraditionalBalance DECIMAL(18,2),
    RothBalance DECIMAL(18,2),
    MonthlyTaxableContribution DECIMAL(18,2),
    MonthlyTraditionalContribution DECIMAL(18,2),
    MonthlyRothContribution DECIMAL(18,2),
    AnnualGrowthRate DECIMAL(5,2),
    Years INT,
	RetirementAge int
--	LastUpdateDate DateTime
) AS jsonData
where CalculatorType like '%with%'

  --truncate table [dev-glamourre].[finplan].[FinPlan]
 -- "TraditionalTaxRate":22.0,"AnnualWithdrawal":120000,"InflationRate":2.5,"InvestmentReturn":5.0,"HasPartialRetirement":false,"PartialRetirementEndAge":70,"PartialRetirementIncome":25000,"Strategy":2,"PriorityStrategy":0,"WithdrawalOrder":["Taxable","Traditional","Roth"],"WithdrawalPercentage":4.0,"PlanYears":30,"LastUpdateDate":"2025-09-01T02:59:12.9365192Z"}