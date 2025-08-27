SELECT TOP (1000)
    [Id],
    [UserGuid],
    [CalculatorType],
    [Data],
    jsonData.CurrentAge,
    jsonData.RetirementAge,
    jsonData.InitialTaxableAmount,
    jsonData.InitialTraditionalAmount,
    jsonData.InitialRothAmount,
    jsonData.MonthlyTaxableContribution,
    jsonData.MonthlyTraditionalContribution,
    jsonData.MonthlyRothContribution,
    jsonData.AnnualGrowthRate,
    jsonData.Years
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
    Years INT
) AS jsonData

  --truncate table [dev-glamourre].[finplan].[FinPlan]
