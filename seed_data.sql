-- ============================================================
-- B3ly Store — Seed Categories & Products
-- Run this in SSMS against: B3lyDb
-- Safe to run multiple times (uses IF NOT EXISTS checks)
-- ============================================================

USE B3lyDb;
GO

-- ── 1. Categories ────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Phones')
    INSERT INTO Categories (Name) VALUES ('Phones');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Laptops')
    INSERT INTO Categories (Name) VALUES ('Laptops');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Electronics')
    INSERT INTO Categories (Name) VALUES ('Electronics');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Clothing')
    INSERT INTO Categories (Name) VALUES ('Clothing');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Books')
    INSERT INTO Categories (Name) VALUES ('Books');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Gaming')
    INSERT INTO Categories (Name) VALUES ('Gaming');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Home & Kitchen')
    INSERT INTO Categories (Name) VALUES ('Home & Kitchen');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Sports & Fitness')
    INSERT INTO Categories (Name) VALUES ('Sports & Fitness');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Beauty')
    INSERT INTO Categories (Name) VALUES ('Beauty');

-- ── 2. Helper: grab category IDs ─────────────────────────────
DECLARE @Phones      INT = (SELECT CategoryId FROM Categories WHERE Name = 'Phones');
DECLARE @Laptops     INT = (SELECT CategoryId FROM Categories WHERE Name = 'Laptops');
DECLARE @Electronics INT = (SELECT CategoryId FROM Categories WHERE Name = 'Electronics');
DECLARE @Clothing    INT = (SELECT CategoryId FROM Categories WHERE Name = 'Clothing');
DECLARE @Books       INT = (SELECT CategoryId FROM Categories WHERE Name = 'Books');
DECLARE @Gaming      INT = (SELECT CategoryId FROM Categories WHERE Name = 'Gaming');
DECLARE @Home        INT = (SELECT CategoryId FROM Categories WHERE Name = 'Home & Kitchen');
DECLARE @Sports      INT = (SELECT CategoryId FROM Categories WHERE Name = 'Sports & Fitness');
DECLARE @Beauty      INT = (SELECT CategoryId FROM Categories WHERE Name = 'Beauty');

-- ── 3. Products (skip if SKU already exists) ─────────────────

-- Phones
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'APL-IP15P')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Phones, 'iPhone 15 Pro', 'APL-IP15P', 999.99, 50, 1,
 'Apple iPhone 15 Pro with titanium design, A17 Pro chip and 48 MP camera system.',
 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SAM-GS24')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Phones, 'Samsung Galaxy S24 Ultra', 'SAM-GS24', 849.99, 40, 1,
 'Samsung Galaxy S24 Ultra with Galaxy AI and 200 MP camera.',
 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GOG-PXL8')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Phones, 'Google Pixel 8 Pro', 'GOG-PXL8', 799.99, 30, 1,
 'Google Pixel 8 Pro with Tensor G3 chip and Magic Eraser camera feature.',
 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'ONE-12P')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Phones, 'OnePlus 12 Pro', 'ONE-12P', 699.99, 25, 1,
 'OnePlus 12 Pro with Snapdragon 8 Gen 3 and 100W fast charging.',
 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400&h=400&fit=crop', GETUTCDATE());

-- Laptops
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'APL-MBA-M3')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Laptops, 'MacBook Air M3', 'APL-MBA-M3', 1299.99, 25, 1,
 'Ultra-thin MacBook Air with M3 chip and 18-hour battery life.',
 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'DEL-XPS15')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Laptops, 'Dell XPS 15', 'DEL-XPS15', 1199.99, 20, 1,
 'Dell XPS 15 with 4K OLED display and Intel Core i9.',
 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'LEN-X1C')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Laptops, 'Lenovo ThinkPad X1 Carbon', 'LEN-X1C', 1099.99, 18, 1,
 'Business ultrabook with military-grade durability and 12th Gen Intel Core.',
 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'MSF-SB-PRO')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Laptops, 'Microsoft Surface Pro 9', 'MSF-SB-PRO', 1399.99, 15, 1,
 '2-in-1 Surface Pro 9 with Intel Core i7 and detachable keyboard.',
 'https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=400&h=400&fit=crop', GETUTCDATE());

-- Electronics
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SNY-WH5')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Electronics, 'Sony WH-1000XM5', 'SNY-WH5', 349.99, 35, 1,
 'Industry-leading noise-canceling wireless over-ear headphones.',
 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'APL-AW-S9')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Electronics, 'Apple Watch Series 9', 'APL-AW-S9', 399.99, 45, 1,
 'Apple Watch Series 9 with S9 chip, double-tap gesture and Always-On display.',
 'https://images.unsplash.com/photo-1551816230-ef5deaed4a26?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SNY-CAM-A7')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Electronics, 'Sony Alpha A7 IV', 'SNY-CAM-A7', 2499.99, 10, 1,
 'Full-frame mirrorless camera with 33 MP back-illuminated sensor.',
 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'APL-AIRPDS')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Electronics, 'AirPods Pro 2nd Gen', 'APL-AIRPDS', 249.99, 60, 1,
 'Apple AirPods Pro with Active Noise Cancellation and Adaptive Audio.',
 'https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=400&h=400&fit=crop', GETUTCDATE());

-- Clothing
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CLT-MWT')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Clothing, 'Classic White T-Shirt', 'CLT-MWT', 29.99, 100, 1,
 '100% premium cotton essential white crew-neck tee.',
 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CLT-MJN')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Clothing, 'Slim Fit Jeans', 'CLT-MJN', 59.99, 75, 1,
 'Classic slim-fit denim jeans in clean indigo wash.',
 'https://images.unsplash.com/photo-1542272604-787c3835535d?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CLT-WFD')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Clothing, 'Floral Summer Dress', 'CLT-WFD', 49.99, 60, 1,
 'Lightweight floral-print midi dress perfect for summer.',
 'https://images.unsplash.com/photo-1496747611176-843222e1e57c?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CLT-SNK')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Clothing, 'Running Sneakers', 'CLT-SNK', 89.99, 80, 1,
 'Lightweight breathable sneakers with foam cushioning.',
 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&h=400&fit=crop', GETUTCDATE());

-- Books
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BK-CC-001')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Books, 'Clean Code', 'BK-CC-001', 39.99, 150, 1,
 'Robert C. Martin — A handbook of agile software craftsmanship.',
 'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BK-DP-001')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Books, 'Design Patterns', 'BK-DP-001', 44.99, 80, 1,
 'Gang of Four — Elements of reusable object-oriented software.',
 'https://images.unsplash.com/photo-1507842217343-583bb7270b66?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BK-ATOM')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Books, 'Atomic Habits', 'BK-ATOM', 19.99, 200, 1,
 'James Clear — Build good habits and break bad ones.',
 'https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BK-DEEP')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Books, 'Deep Work', 'BK-DEEP', 22.99, 120, 1,
 'Cal Newport — Rules for focused success in a distracted world.',
 'https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=400&h=400&fit=crop', GETUTCDATE());

-- Gaming
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GMG-MOUSE')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Gaming, 'Logitech G Pro X Superlight', 'GMG-MOUSE', 159.99, 55, 1,
 'Ultra-lightweight wireless gaming mouse with HERO 25K sensor.',
 'https://images.unsplash.com/photo-1527814050087-3793815479db?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GMG-KB')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Gaming, 'Mechanical Gaming Keyboard', 'GMG-KB', 129.99, 40, 1,
 'TKL mechanical keyboard with Cherry MX Red switches and per-key RGB.',
 'https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GMG-CTL')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Gaming, 'Xbox Wireless Controller', 'GMG-CTL', 59.99, 70, 1,
 'Official Xbox wireless controller with textured grip and hybrid D-pad.',
 'https://images.unsplash.com/photo-1486401899868-0e435ed85128?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GMG-HSET')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Gaming, 'Gaming Headset', 'GMG-HSET', 89.99, 45, 1,
 '7.1 surround sound gaming headset with noise-canceling microphone.',
 'https://images.unsplash.com/photo-1612198188060-c7c2a3b66eae?w=400&h=400&fit=crop', GETUTCDATE());

-- Home & Kitchen
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HK-COFFEE')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Home, 'De''Longhi Espresso Machine', 'HK-COFFEE', 299.99, 20, 1,
 'Fully automatic espresso machine with built-in conical burr grinder.',
 'https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HK-KNIFE')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Home, 'Chef''s Knife Set', 'HK-KNIFE', 79.99, 35, 1,
 'Professional 8-piece stainless-steel kitchen knife set with wooden block.',
 'https://images.unsplash.com/photo-1593618998160-e34014e67546?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HK-CANDLE')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Home, 'Luxury Scented Candle Set', 'HK-CANDLE', 34.99, 90, 1,
 'Set of 3 hand-poured soy wax candles in calming lavender, vanilla and cedar scents.',
 'https://images.unsplash.com/photo-1603006905003-be475563bc59?w=400&h=400&fit=crop', GETUTCDATE());

-- Sports & Fitness
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SPT-YOGA')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Sports, 'Premium Yoga Mat', 'SPT-YOGA', 44.99, 65, 1,
 '6mm thick non-slip eco-friendly yoga and pilates mat with alignment lines.',
 'https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SPT-DUMB')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Sports, 'Adjustable Dumbbell Set', 'SPT-DUMB', 149.99, 30, 1,
 '5–25 kg adjustable dumbbell set with compact storage rack for home gym.',
 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'SPT-BTTLE')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Sports, 'Insulated Water Bottle', 'SPT-BTTLE', 24.99, 120, 1,
 '32 oz stainless-steel vacuum insulated sports bottle, keeps cold 24h.',
 'https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=400&h=400&fit=crop', GETUTCDATE());

-- Beauty
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BTY-SKIN')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Beauty, 'Vitamin C Face Serum', 'BTY-SKIN', 34.99, 85, 1,
 '20% Vitamin C brightening face serum with hyaluronic acid and niacinamide.',
 'https://images.unsplash.com/photo-1556228578-8c89e6adf883?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BTY-PERF')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Beauty, 'Floral Eau de Parfum', 'BTY-PERF', 89.99, 40, 1,
 'Elegant floral fragrance with top notes of rose and jasmine, base of musk.',
 'https://images.unsplash.com/photo-1541643600914-78b084683702?w=400&h=400&fit=crop', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BTY-LIP')
INSERT INTO Products (CategoryId, Name, SKU, Price, StockQuantity, IsActive, Description, ImageUrl, CreatedAt) VALUES
(@Beauty, 'Matte Lipstick Collection', 'BTY-LIP', 19.99, 100, 1,
 'Long-lasting matte lipstick set with 6 trending shades for all skin tones.',
 'https://images.unsplash.com/photo-1586495777744-4e6232bf0b9a?w=400&h=400&fit=crop', GETUTCDATE());

-- ── Done ──────────────────────────────────────────────────────
SELECT
    c.Name        AS Category,
    COUNT(p.ProductId) AS Products
FROM Categories c
LEFT JOIN Products p ON p.CategoryId = c.CategoryId
GROUP BY c.Name
ORDER BY c.Name;
