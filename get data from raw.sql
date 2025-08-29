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

  --truncate table [dev-glamourre].[finplan].[FinPlan]
