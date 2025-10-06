-- Insert City Templates
INSERT INTO [finplan].[CityTemplates] 
    ([CityId], [CityName], [Country], [Currency], [CostOfLivingIndex], [CreatedAt], [UpdatedAt], [CreatedBy])
VALUES
    ('nyc-usa', 'New York City', 'United States', 'USD', 100.00, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('london-uk', 'London', 'United Kingdom', 'GBP', 95.50, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('tokyo-japan', 'Tokyo', 'Japan', 'JPY', 92.30, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('sydney-aus', 'Sydney', 'Australia', 'AUD', 89.70, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('toronto-can', 'Toronto', 'Canada', 'CAD', 85.20, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('singapore-sg', 'Singapore', 'Singapore', 'SGD', 93.80, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('weston-fl-usa', 'Weston, FL', 'United States', 'USD', 78.50, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('mumbai-india', 'Mumbai', 'India', 'INR', 45.30, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('bangalore-india', 'Bangalore', 'India', 'INR', 42.80, GETUTCDATE(), GETUTCDATE(), 'System'),
    ('hubli-india', 'Hubli', 'India', 'INR', 28.50, GETUTCDATE(), GETUTCDATE(), 'System');
GO

-- Insert Demographic Profiles for NYC
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'nyc-young-single',
        'nyc-usa',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":2800,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":150,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":400,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Metro Card","CurrentValue":127,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":300,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":50,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Gym Membership","CurrentValue":100,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'nyc-family-2kids',
        'nyc-usa',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[5,8]',
        '[{"Category":"Housing","Subcategory":"Rent/Mortgage","CurrentValue":4500,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":250,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Property Tax","CurrentValue":800,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":1200,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":600,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Payment","CurrentValue":450,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Gas","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Insurance","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":800,"IncludeInRetirement":true},{"Category":"Childcare","Subcategory":"Daycare","CurrentValue":2500,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"Activities","CurrentValue":300,"IncludeInRetirement":false,"RetirementExclusionReason":0}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'nyc-retired-couple',
        'nyc-usa',
        'Retired Couple',
        60, 85,
        1, -- Married
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":3200,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":800,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Metro/Taxi","CurrentValue":150,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Medicare Supplement","CurrentValue":400,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Prescriptions","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Travel","CurrentValue":600,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Hobbies","CurrentValue":250,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for London
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'london-young-single',
        'london-uk',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":1800,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Council Tax","CurrentValue":150,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":120,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":300,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":250,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Oyster Card","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Private Insurance","CurrentValue":100,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":40,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'london-family-2kids',
        'london-uk',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[6,9]',
        '[{"Category":"Housing","Subcategory":"Mortgage","CurrentValue":2500,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Housing","Subcategory":"Council Tax","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":600,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":350,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Payment","CurrentValue":350,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Petrol","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Insurance","CurrentValue":120,"IncludeInRetirement":true},{"Category":"Childcare","Subcategory":"After School Club","CurrentValue":800,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"School Uniforms","CurrentValue":100,"IncludeInRetirement":false,"RetirementExclusionReason":0}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for Tokyo
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'tokyo-young-single',
        'tokyo-japan',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":90000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":12000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":40000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":30000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Train Pass","CurrentValue":15000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":3000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'tokyo-family-1kid',
        'tokyo-japan',
        'Family with 1 Child',
        30, 50,
        1, -- Married
        1,
        '[7]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":150000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":18000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":70000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":40000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Train Pass","CurrentValue":20000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":15000,"IncludeInRetirement":true},{"Category":"Education","Subcategory":"Juku (Cram School)","CurrentValue":30000,"IncludeInRetirement":false,"RetirementExclusionReason":0}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for Weston, FL
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'weston-young-single',
        'weston-fl-usa',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":1800,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":400,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":350,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Payment","CurrentValue":400,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Gas","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":150,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":250,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":60,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Gym Membership","CurrentValue":80,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'weston-family-2kids',
        'weston-fl-usa',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[7,10]',
        '[{"Category":"Housing","Subcategory":"Mortgage","CurrentValue":3200,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Housing","Subcategory":"Property Tax","CurrentValue":650,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"HOA Fees","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":250,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":1000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Payment","CurrentValue":600,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Gas","CurrentValue":300,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":220,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":700,"IncludeInRetirement":true},{"Category":"Childcare","Subcategory":"After School Care","CurrentValue":800,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"Activities","CurrentValue":400,"IncludeInRetirement":false,"RetirementExclusionReason":0}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'weston-retired-couple',
        'weston-fl-usa',
        'Retired Couple',
        60, 85,
        1, -- Married
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent/Mortgage","CurrentValue":2200,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Property Tax","CurrentValue":550,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"HOA Fees","CurrentValue":200,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":220,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":700,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":450,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Gas","CurrentValue":120,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":160,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Medicare Supplement","CurrentValue":350,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Prescriptions","CurrentValue":180,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Travel","CurrentValue":500,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Golf/Hobbies","CurrentValue":200,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for Mumbai
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'mumbai-young-single',
        'mumbai-india',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":35000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":3500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Auto/Metro","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Cab/Ride Share","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Gym Membership","CurrentValue":2000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'mumbai-family-2kids',
        'mumbai-india',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[6,9]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":65000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":18000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":10000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car EMI","CurrentValue":18000,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Fuel","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":12000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Education","Subcategory":"School Fees","CurrentValue":25000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"Tuition","CurrentValue":10000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":8000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'mumbai-retired-couple',
        'mumbai-india',
        'Retired Couple',
        60, 85,
        1, -- Married
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":45000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":5000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":15000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Cab/Auto","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":10000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":10000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Medicines","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Travel","CurrentValue":10000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for Bangalore
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'bangalore-young-single',
        'bangalore-india',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":25000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":5000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Metro/Bus","CurrentValue":2000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Cab/Ride Share","CurrentValue":3500,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":2000,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":1200,"IncludeInRetirement":true},{"Category":"Personal","Subcategory":"Gym Membership","CurrentValue":1500,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'bangalore-family-2kids',
        'bangalore-india',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[5,8]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":50000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":4500,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":15000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car EMI","CurrentValue":15000,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Fuel","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":2000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":10000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Education","Subcategory":"School Fees","CurrentValue":20000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"Tuition","CurrentValue":8000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":6000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'bangalore-retired-couple',
        'bangalore-india',
        'Retired Couple',
        60, 85,
        1, -- Married
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":35000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":3500,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":2000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":12000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Cab/Auto","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Medicines","CurrentValue":5000,"IncludeInRetirement":true},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Travel","CurrentValue":8000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO

-- Insert Demographic Profiles for Hubli
INSERT INTO [finplan].[DemographicProfiles]
    ([ProfileId], [CityId], [ProfileName], [AgeMin], [AgeMax], [MaritalStatus], [ChildrenCount], [ChildrenAgesJSON], [SampleExpensesJSON], [CreatedAt], [UpdatedAt])
VALUES
    (
        'hubli-young-single',
        'hubli-india',
        'Young Professional - Single',
        22, 35,
        0, -- Single
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":12000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Two-Wheeler EMI","CurrentValue":4000,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Fuel","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Auto/Cab","CurrentValue":1000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Subscriptions","CurrentValue":800,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'hubli-family-2kids',
        'hubli-india',
        'Family with 2 Children',
        30, 50,
        1, -- Married
        2,
        '[6,9]',
        '[{"Category":"Housing","Subcategory":"Rent/Mortgage","CurrentValue":20000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":2500,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":1000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":10000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car EMI","CurrentValue":10000,"IncludeInRetirement":false,"RetirementExclusionReason":1},{"Category":"Transportation","Subcategory":"Fuel","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Car Insurance","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":6000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":4000,"IncludeInRetirement":true},{"Category":"Education","Subcategory":"School Fees","CurrentValue":10000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Education","Subcategory":"Tuition","CurrentValue":5000,"IncludeInRetirement":false,"RetirementExclusionReason":0},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":3000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    ),
    (
        'hubli-retired-couple',
        'hubli-india',
        'Retired Couple',
        60, 85,
        1, -- Married
        0,
        '[]',
        '[{"Category":"Housing","Subcategory":"Rent","CurrentValue":15000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Utilities","CurrentValue":2000,"IncludeInRetirement":true},{"Category":"Housing","Subcategory":"Maintenance","CurrentValue":1000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Groceries","CurrentValue":8000,"IncludeInRetirement":true},{"Category":"Food","Subcategory":"Dining Out","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Auto/Cab","CurrentValue":1500,"IncludeInRetirement":true},{"Category":"Transportation","Subcategory":"Driver Salary","CurrentValue":5000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Insurance","CurrentValue":5000,"IncludeInRetirement":true},{"Category":"Healthcare","Subcategory":"Medicines","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Household","Subcategory":"Maid/Help","CurrentValue":3000,"IncludeInRetirement":true},{"Category":"Entertainment","Subcategory":"Travel/Hobbies","CurrentValue":4000,"IncludeInRetirement":true}]',
        GETUTCDATE(),
        GETUTCDATE()
    );
GO
