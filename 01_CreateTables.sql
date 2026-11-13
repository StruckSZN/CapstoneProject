/* MealWise normalized schema. Run against MealPlannerDb. */
USE MealPlannerDb;
GO

CREATE TABLE dbo.AppUsers
(
    UserId       int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
    DisplayName  nvarchar(100) NOT NULL,
    Email        nvarchar(254) NULL,
    CreatedAt    datetime2(0) NOT NULL CONSTRAINT DF_AppUsers_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO
CREATE UNIQUE INDEX UX_AppUsers_Email
    ON dbo.AppUsers(Email) WHERE Email IS NOT NULL;
GO

CREATE TABLE dbo.DietaryRestrictions
(
    RestrictionId   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DietaryRestrictions PRIMARY KEY,
    RestrictionName nvarchar(80) NOT NULL,
    Description     nvarchar(300) NULL,
    CONSTRAINT UQ_DietaryRestrictions_Name UNIQUE (RestrictionName)
);
GO

CREATE TABLE dbo.UserDietaryRestrictions
(
    UserId        int NOT NULL,
    RestrictionId int NOT NULL,
    Notes         nvarchar(250) NULL,
    CONSTRAINT PK_UserDietaryRestrictions PRIMARY KEY (UserId, RestrictionId),
    CONSTRAINT FK_UserDietaryRestrictions_User FOREIGN KEY (UserId)
        REFERENCES dbo.AppUsers(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserDietaryRestrictions_Restriction FOREIGN KEY (RestrictionId)
        REFERENCES dbo.DietaryRestrictions(RestrictionId)
);
GO

CREATE TABLE dbo.MealTypes
(
    MealTypeId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MealTypes PRIMARY KEY,
    TypeName   nvarchar(40) NOT NULL,
    SortOrder  tinyint NOT NULL,
    CONSTRAINT UQ_MealTypes_TypeName UNIQUE (TypeName)
);
GO

CREATE TABLE dbo.RecipeCategories
(
    CategoryId   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RecipeCategories PRIMARY KEY,
    CategoryName nvarchar(80) NOT NULL,
    CONSTRAINT UQ_RecipeCategories_CategoryName UNIQUE (CategoryName)
);
GO

CREATE TABLE dbo.Cuisines
(
    CuisineId   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cuisines PRIMARY KEY,
    CuisineName nvarchar(80) NOT NULL,
    CONSTRAINT UQ_Cuisines_CuisineName UNIQUE (CuisineName)
);
GO

CREATE TABLE dbo.Ingredients
(
    IngredientId   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ingredients PRIMARY KEY,
    IngredientName nvarchar(120) NOT NULL,
    CONSTRAINT UQ_Ingredients_IngredientName UNIQUE (IngredientName)
);
GO

CREATE TABLE dbo.MeasurementUnits
(
    UnitId       int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MeasurementUnits PRIMARY KEY,
    UnitName     nvarchar(50) NOT NULL,
    Abbreviation nvarchar(15) NOT NULL,
    CONSTRAINT UQ_MeasurementUnits_UnitName UNIQUE (UnitName),
    CONSTRAINT UQ_MeasurementUnits_Abbreviation UNIQUE (Abbreviation)
);
GO

CREATE TABLE dbo.Recipes
(
    RecipeId       int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Recipes PRIMARY KEY,
    RecipeName     nvarchar(160) NOT NULL,
    CategoryId     int NULL,
    CuisineId      int NULL,
    Instructions   nvarchar(max) NULL,
    PrepMinutes    smallint NULL,
    CookMinutes    smallint NULL,
    DefaultServings tinyint NOT NULL CONSTRAINT DF_Recipes_DefaultServings DEFAULT (4),
    ImageUrl       nvarchar(1000) NULL,
    SourceName     nvarchar(80) NULL,
    SourceRecipeId nvarchar(50) NULL,
    SourceUrl      nvarchar(1000) NULL,
    IsFavorite     bit NOT NULL CONSTRAINT DF_Recipes_IsFavorite DEFAULT (0),
    DateAdded      datetime2(0) NOT NULL CONSTRAINT DF_Recipes_DateAdded DEFAULT SYSUTCDATETIME(),
    UpdatedAt      datetime2(0) NULL,
    CONSTRAINT FK_Recipes_Category FOREIGN KEY (CategoryId) REFERENCES dbo.RecipeCategories(CategoryId),
    CONSTRAINT FK_Recipes_Cuisine FOREIGN KEY (CuisineId) REFERENCES dbo.Cuisines(CuisineId),
    CONSTRAINT CK_Recipes_PrepMinutes CHECK (PrepMinutes IS NULL OR PrepMinutes >= 0),
    CONSTRAINT CK_Recipes_CookMinutes CHECK (CookMinutes IS NULL OR CookMinutes >= 0),
    CONSTRAINT CK_Recipes_DefaultServings CHECK (DefaultServings > 0)
);
GO
CREATE UNIQUE INDEX UX_Recipes_ExternalSource
    ON dbo.Recipes(SourceName, SourceRecipeId)
    WHERE SourceName IS NOT NULL AND SourceRecipeId IS NOT NULL;
GO
CREATE INDEX IX_Recipes_RecipeName ON dbo.Recipes(RecipeName);
GO

CREATE TABLE dbo.RecipeIngredients
(
    RecipeIngredientId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RecipeIngredients PRIMARY KEY,
    RecipeId           int NOT NULL,
    IngredientId       int NOT NULL,
    Quantity           decimal(10,2) NULL,
    UnitId             int NULL,
    MeasureText        nvarchar(100) NULL,
    PreparationNote    nvarchar(150) NULL,
    IsOptional         bit NOT NULL CONSTRAINT DF_RecipeIngredients_IsOptional DEFAULT (0),
    CONSTRAINT FK_RecipeIngredients_Recipe FOREIGN KEY (RecipeId)
        REFERENCES dbo.Recipes(RecipeId) ON DELETE CASCADE,
    CONSTRAINT FK_RecipeIngredients_Ingredient FOREIGN KEY (IngredientId)
        REFERENCES dbo.Ingredients(IngredientId),
    CONSTRAINT FK_RecipeIngredients_Unit FOREIGN KEY (UnitId)
        REFERENCES dbo.MeasurementUnits(UnitId),
    CONSTRAINT CK_RecipeIngredients_Quantity CHECK (Quantity IS NULL OR Quantity > 0)
);
GO
CREATE INDEX IX_RecipeIngredients_RecipeId ON dbo.RecipeIngredients(RecipeId);
CREATE INDEX IX_RecipeIngredients_IngredientId ON dbo.RecipeIngredients(IngredientId);
GO

CREATE TABLE dbo.RecipeDietaryRestrictions
(
    RecipeId      int NOT NULL,
    RestrictionId int NOT NULL,
    CONSTRAINT PK_RecipeDietaryRestrictions PRIMARY KEY (RecipeId, RestrictionId),
    CONSTRAINT FK_RecipeDietaryRestrictions_Recipe FOREIGN KEY (RecipeId)
        REFERENCES dbo.Recipes(RecipeId) ON DELETE CASCADE,
    CONSTRAINT FK_RecipeDietaryRestrictions_Restriction FOREIGN KEY (RestrictionId)
        REFERENCES dbo.DietaryRestrictions(RestrictionId)
);
GO

CREATE TABLE dbo.MealPlans
(
    MealPlanId    int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MealPlans PRIMARY KEY,
    UserId        int NOT NULL,
    PlanName      nvarchar(100) NOT NULL,
    WeekStartDate date NOT NULL,
    CreatedAt     datetime2(0) NOT NULL CONSTRAINT DF_MealPlans_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MealPlans_User FOREIGN KEY (UserId)
        REFERENCES dbo.AppUsers(UserId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_MealPlans_User_Week ON dbo.MealPlans(UserId, WeekStartDate);
GO

CREATE TABLE dbo.MealPlanEntries
(
    MealPlanEntryId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MealPlanEntries PRIMARY KEY,
    MealPlanId      int NOT NULL,
    RecipeId        int NOT NULL,
    MealTypeId      int NOT NULL,
    PlannedDate     date NOT NULL,
    Servings        tinyint NOT NULL CONSTRAINT DF_MealPlanEntries_Servings DEFAULT (2),
    Notes           nvarchar(250) NULL,
    CONSTRAINT FK_MealPlanEntries_MealPlan FOREIGN KEY (MealPlanId)
        REFERENCES dbo.MealPlans(MealPlanId) ON DELETE CASCADE,
    CONSTRAINT FK_MealPlanEntries_Recipe FOREIGN KEY (RecipeId)
        REFERENCES dbo.Recipes(RecipeId),
    CONSTRAINT FK_MealPlanEntries_MealType FOREIGN KEY (MealTypeId)
        REFERENCES dbo.MealTypes(MealTypeId),
    CONSTRAINT CK_MealPlanEntries_Servings CHECK (Servings > 0),
    CONSTRAINT UQ_MealPlanEntries_Plan_Date_MealType UNIQUE (MealPlanId, PlannedDate, MealTypeId)
);
GO
CREATE INDEX IX_MealPlanEntries_Date ON dbo.MealPlanEntries(PlannedDate);
GO

CREATE TABLE dbo.GroceryLists
(
    GroceryListId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GroceryLists PRIMARY KEY,
    MealPlanId    int NOT NULL,
    ListName      nvarchar(100) NOT NULL,
    CreatedAt     datetime2(0) NOT NULL CONSTRAINT DF_GroceryLists_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_GroceryLists_MealPlan UNIQUE (MealPlanId),
    CONSTRAINT FK_GroceryLists_MealPlan FOREIGN KEY (MealPlanId)
        REFERENCES dbo.MealPlans(MealPlanId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.GroceryListItems
(
    GroceryListItemId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GroceryListItems PRIMARY KEY,
    GroceryListId     int NOT NULL,
    IngredientId      int NOT NULL,
    Quantity          decimal(10,2) NULL,
    UnitId            int NULL,
    IsPurchased       bit NOT NULL CONSTRAINT DF_GroceryListItems_IsPurchased DEFAULT (0),
    Notes             nvarchar(200) NULL,
    CONSTRAINT FK_GroceryListItems_GroceryList FOREIGN KEY (GroceryListId)
        REFERENCES dbo.GroceryLists(GroceryListId) ON DELETE CASCADE,
    CONSTRAINT FK_GroceryListItems_Ingredient FOREIGN KEY (IngredientId)
        REFERENCES dbo.Ingredients(IngredientId),
    CONSTRAINT FK_GroceryListItems_Unit FOREIGN KEY (UnitId)
        REFERENCES dbo.MeasurementUnits(UnitId),
    CONSTRAINT CK_GroceryListItems_Quantity CHECK (Quantity IS NULL OR Quantity > 0)
);
GO
CREATE INDEX IX_GroceryListItems_List ON dbo.GroceryListItems(GroceryListId, IsPurchased);
GO

/* 15 tables total. All relationships are enforced with primary/foreign keys. */
