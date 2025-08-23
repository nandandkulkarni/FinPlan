    public List<YearlyBreakdown> GetYearlyBreakdown(SavingsCalculatorModel model)
    {
        var breakdown = new List<YearlyBreakdown>();
        
        // Get tax rates based on bracket
        decimal ordinaryTaxRate = GetOrdinaryTaxRate(model.TaxBracket);
        decimal longTermGainsTaxRate = GetLongTermGainsTaxRate(model.TaxBracket);
        
        // Get income distribution based on type
        var (qualifiedPercent, nonQualifiedPercent, longTermPercent, shortTermPercent) = 
            GetIncomeDistribution(model.TaxableIncomeType);
        
        // Use separate balances for each account type
        decimal taxableBalance = model.InitialTaxableAmount;
        decimal traditionalBalance = model.InitialTraditionalAmount;
        decimal rothBalance = model.InitialRothAmount;
        
        decimal monthlyRate = model.AnnualGrowthRate / 100 / 12;
        decimal monthlyTaxableContribution = model.MonthlyTaxableContribution;
        decimal monthlyTraditionalContribution = model.MonthlyTraditionalContribution;
        decimal monthlyRothContribution = model.MonthlyRothContribution;

        for (int year = 1; year <= model.Years; year++)
        {
            // Track yearly values
            decimal yearlyTaxableInterest = 0;
            decimal yearlyTraditionalInterest = 0;
            decimal yearlyRothInterest = 0;
            decimal yearlyTaxableContribution = monthlyTaxableContribution * 12;
            decimal yearlyTraditionalContribution = monthlyTraditionalContribution * 12;
            decimal yearlyRothContribution = monthlyRothContribution * 12;
            
            // Track tax breakdown
            decimal yearlyQualifiedDividends = 0;
            decimal yearlyNonQualifiedIncome = 0;
            decimal yearlyLongTermGains = 0;
            decimal yearlyShortTermGains = 0;
            decimal yearlyTaxesPaid = 0;
            
            decimal startTaxableBalance = taxableBalance;
            decimal startTraditionalBalance = traditionalBalance;
            decimal startRothBalance = rothBalance;

            // Calculate month by month for this year
            for (int month = 1; month <= 12; month++)
            {
                // Taxable account growth
                decimal monthlyTaxableInterest = taxableBalance * monthlyRate;
                yearlyTaxableInterest += monthlyTaxableInterest;
                taxableBalance += monthlyTaxableInterest + monthlyTaxableContribution;
                
                // Traditional (tax-deferred) account growth
                decimal monthlyTraditionalInterest = traditionalBalance * monthlyRate;
                yearlyTraditionalInterest += monthlyTraditionalInterest;
                traditionalBalance += monthlyTraditionalInterest + monthlyTraditionalContribution;
                
                // Roth account growth
                decimal monthlyRothInterest = rothBalance * monthlyRate;
                yearlyRothInterest += monthlyRothInterest;
                rothBalance += monthlyRothInterest + monthlyRothContribution;
            }
            
            // Calculate tax impact on taxable account
            yearlyQualifiedDividends = yearlyTaxableInterest * qualifiedPercent;
            yearlyNonQualifiedIncome = yearlyTaxableInterest * nonQualifiedPercent;
            yearlyLongTermGains = yearlyTaxableInterest * longTermPercent;
            yearlyShortTermGains = yearlyTaxableInterest * shortTermPercent;
            
            // Calculate taxes
            decimal qualifiedDividendsTax = yearlyQualifiedDividends * longTermGainsTaxRate;
            decimal nonQualifiedTax = yearlyNonQualifiedIncome * ordinaryTaxRate;
            decimal longTermGainsTax = yearlyLongTermGains * longTermGainsTaxRate;
            decimal shortTermGainsTax = yearlyShortTermGains * ordinaryTaxRate;
            
            yearlyTaxesPaid = qualifiedDividendsTax + nonQualifiedTax + longTermGainsTax + shortTermGainsTax;
            
            // Reduce taxable balance by taxes paid
            taxableBalance -= yearlyTaxesPaid;
            
            // Calculate total values
            decimal totalYearlyBalance = taxableBalance + traditionalBalance + rothBalance;
            decimal totalYearlyInterest = yearlyTaxableInterest + yearlyTraditionalInterest + yearlyRothInterest - yearlyTaxesPaid;
            decimal totalYearlyContribution = yearlyTaxableContribution + yearlyTraditionalContribution + yearlyRothContribution;

            breakdown.Add(new YearlyBreakdown
            {
                Year = year,
                Balance = Math.Round(totalYearlyBalance, 2),
                InterestEarned = Math.Round(totalYearlyInterest, 2),
                ContributionsThisYear = totalYearlyContribution,
                TaxableBalance = Math.Round(taxableBalance, 2),
                TaxDeferredBalance = Math.Round(traditionalBalance, 2),
                RothBalance = Math.Round(rothBalance, 2),
                TaxableInterest = Math.Round(yearlyTaxableInterest - yearlyTaxesPaid, 2),
                TaxDeferredInterest = Math.Round(yearlyTraditionalInterest, 2),
                RothInterest = Math.Round(yearlyRothInterest, 2),
                TaxableContribution = yearlyTaxableContribution,
                TaxDeferredContribution = yearlyTraditionalContribution,
                RothContribution = yearlyRothContribution,
                QualifiedDividends = Math.Round(yearlyQualifiedDividends, 2),
                NonQualifiedIncome = Math.Round(yearlyNonQualifiedIncome, 2),
                LongTermGains = Math.Round(yearlyLongTermGains, 2),
                ShortTermGains = Math.Round(yearlyShortTermGains, 2),
                TaxesPaid = Math.Round(yearlyTaxesPaid, 2)
            });
        }

        return breakdown;
    }
