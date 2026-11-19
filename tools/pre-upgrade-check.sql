/*
  Run this against your EXISTING BookCenter database BEFORE `dotnet ef database update`.
  It only reads. Rows marked BLOCKER would make the upgrade migration
  (SoftDeleteStockAndConstraints) fail; fix them first. (If it does fail, nothing is changed:
  each migration runs in a transaction.) WARNING rows don't block the upgrade, but the app
  will reject that value the next time the record is edited.
  No rows at all = safe to upgrade.
*/
SELECT 'BLOCKER: Book ISBN longer than 13 characters' AS Problem, CAST(Id AS nvarchar(20)) AS Id, ISBN AS Value
FROM Books WHERE LEN(ISBN) > 13
UNION ALL
SELECT 'WARNING: Book ISBN is not 13 digits', CAST(Id AS nvarchar(20)), ISBN
FROM Books WHERE LEN(ISBN) <= 13 AND (LEN(ISBN) <> 13 OR ISBN LIKE '%[^0-9]%')
UNION ALL
SELECT 'BLOCKER: Duplicate ISBN (active books)', CAST(MIN(Id) AS nvarchar(20)), ISBN
FROM Books WHERE IsDeleted = 0 GROUP BY ISBN HAVING COUNT(*) > 1
UNION ALL
SELECT 'BLOCKER: Book name longer than 200', CAST(Id AS nvarchar(20)), LEFT(Name, 60) FROM Books WHERE LEN(Name) > 200
UNION ALL
SELECT 'BLOCKER: Author name longer than 150', CAST(Id AS nvarchar(20)), LEFT(Name, 60) FROM Authors WHERE LEN(Name) > 150
UNION ALL
SELECT 'BLOCKER: Author biography longer than 2000', CAST(Id AS nvarchar(20)), LEFT(Name, 60) FROM Authors WHERE LEN(Biography) > 2000
UNION ALL
SELECT 'BLOCKER: Publisher name longer than 150', CAST(Id AS nvarchar(20)), LEFT(Name, 60) FROM Publishers WHERE LEN(Name) > 150
UNION ALL
SELECT 'BLOCKER: Duplicate publisher name (active)', CAST(MIN(Id) AS nvarchar(20)), Name
FROM Publishers WHERE IsDeleted = 0 GROUP BY Name HAVING COUNT(*) > 1
UNION ALL
SELECT 'BLOCKER: Reader name/surname longer than 100', CAST(Id AS nvarchar(20)), LEFT(Name + ' ' + Surname, 60)
FROM Readers WHERE LEN(Name) > 100 OR LEN(Surname) > 100
UNION ALL
SELECT 'BLOCKER: Reader phone longer than 30', CAST(Id AS nvarchar(20)), Phone FROM Readers WHERE LEN(Phone) > 30
UNION ALL
SELECT 'BLOCKER: Reader email longer than 256', CAST(Id AS nvarchar(20)), LEFT(Email, 60) FROM Readers WHERE LEN(Email) > 256
UNION ALL
SELECT 'BLOCKER: Duplicate reader email (active)', CAST(MIN(Id) AS nvarchar(20)), Email
FROM Readers WHERE IsDeleted = 0 GROUP BY Email HAVING COUNT(*) > 1
UNION ALL
-- The new check constraint requires 0 <= CopiesAvailable <= TotalCopies.
SELECT 'BLOCKER: Negative CopiesAvailable', CAST(Id AS nvarchar(20)), CAST(CopiesAvailable AS nvarchar(20)) FROM Books WHERE CopiesAvailable < 0;
