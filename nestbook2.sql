
CREATE TABLE [Category] (
  [id] int IDENTITY(1, 1) PRIMARY KEY,
  [name] nvarchar(50),
  [parent_category_id] int
)
GO

CREATE TABLE [ParentCategory] (
  [id] int IDENTITY(1, 1) PRIMARY KEY,
  [type] nvarchar(50)
)
GO

-- Insert Books
CREATE TABLE [Book] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [book_name] nvarchar(100),
  [isbn] varchar(20),
  [cover] nvarchar(15),
  [import_price] decimal(12,2),
  [first_price] decimal(12,2),
  [second_price] decimal(12,2),
  [image_url] varchar(255),
  [author] nvarchar(50),
  [description] NVARCHAR(MAX),
  [year_published] int,
  [rating] decimal(3,1),
  [pages] int,
  [category_id] int,
  [publisher_id] int,
  [quantity] int
)
GO

CREATE TABLE [Publisher] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [name] nvarchar(50)
)
GO

CREATE TABLE [BookRating] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [book_id] int,
  [user_id] int,
  [rating] int,
  [create_at] datetime
)
GO

-- Insert Books

CREATE TABLE [BookComment] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [book_id] int,
  [user_id] int,
  [content] nvarchar(300),
  [create_at] datetime
)
GO

CREATE TABLE [Cart] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [user_id] int,
  [book_id] int,
  [quantity] int
)
GO

CREATE TABLE [Order] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [user_id] int,
  [name] nvarchar(30),
  [address] nvarchar(50),
  [phone] varchar(10),
  [status] varchar(10),
  [from] int,
  [create_at] datetime
)
GO

CREATE TABLE [OrderDetail] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [order_id] int,
  [book_id] int,
  [quantity] int
)
GO

CREATE TABLE [PaymentType] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [type] varchar(20)
)
GO

CREATE TABLE [Payment] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [order_id] int,
  [payment_type_id] int,
  [total_price] int,
  [status] varchar(10),
  [create_at] datetime,
  TransactionRef VARCHAR(255),
  VnpTransactionNo VARCHAR(255),
  VnpResponseCode VARCHAR(255)
)
GO

CREATE TABLE [Voucher] (
  [id] int IDENTITY (1, 1) PRIMARY KEY,
  [voucher_code] varchar(30),
  [type] bit,
  [create_at] datetime,
  [expired_at] datetime,
  [usage_limit] int,
  [used_count] int,
  [value] decimal(10, 2)
)
GO

CREATE TABLE [OrderVoucher] (
  [voucher_id] int,
  [order_id] int,
  PRIMARY KEY ([voucher_id], [order_id])
)
GO

ALTER TABLE [Cart] ADD FOREIGN KEY ([book_id]) REFERENCES [Book] ([id])
GO

ALTER TABLE [BookComment] ADD FOREIGN KEY ([book_id]) REFERENCES [Book] ([id])
GO

ALTER TABLE [Payment] ADD FOREIGN KEY ([payment_type_id]) REFERENCES [PaymentType] ([id])
GO

ALTER TABLE [Payment] ADD FOREIGN KEY ([order_id]) REFERENCES [Order] ([id])
GO

ALTER TABLE [OrderVoucher] ADD FOREIGN KEY ([order_id]) REFERENCES [Order] ([id])
GO

ALTER TABLE [OrderVoucher] ADD FOREIGN KEY ([voucher_id]) REFERENCES [Voucher] ([id])
GO

ALTER TABLE [OrderDetail] ADD FOREIGN KEY ([order_id]) REFERENCES [Order] ([id])
GO

ALTER TABLE [OrderDetail] ADD FOREIGN KEY ([book_id]) REFERENCES [Book] ([id])
GO

ALTER TABLE [BookRating] ADD FOREIGN KEY ([book_id]) REFERENCES [Book] ([id])
GO

ALTER TABLE [Book] ADD FOREIGN KEY ([category_id]) REFERENCES [Category] ([id])
GO

ALTER TABLE [Category] ADD FOREIGN KEY ([parent_category_id]) REFERENCES [ParentCategory] ([id])
GO

ALTER TABLE [Book] ADD FOREIGN KEY ([publisher_id]) REFERENCES [Publisher] ([id])
GO

-- Tắt kiểm tra FK trên toàn database
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Xóa dữ liệu trong bảng
DELETE FROM ParentCategory
DELETE FROM Category
DELETE FROM Publisher
DELETE FROM Book

-- Bật lại kiểm tra FK
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"


DBCC CHECKIDENT ('ParentCategory', RESEED, 0);
DBCC CHECKIDENT ('Category', RESEED, 0);
DBCC CHECKIDENT ('Publisher', RESEED, 0);
DBCC CHECKIDENT ('Book', RESEED, 0);

-- Insert Parent Categories
INSERT INTO [ParentCategory] ([type]) VALUES
(N'Văn học'),
(N'Kinh tế'),
(N'Khoa học'),
(N'Lịch sử'),
(N'Tâm lý'),
(N'Kỹ năng'),
(N'Thiếu nhi'),
(N'Triết học'),
(N'Y học'),
(N'Công nghệ');

-- Insert Categories
INSERT INTO [Category] ([name], [parent_category_id]) VALUES
(N'Tiểu thuyết', 1),
(N'Truyện ngắn', 1),
(N'Thơ ca', 1),
(N'Marketing', 2),
(N'Quản trị', 2),
(N'Đầu tư', 2),
(N'Vật lý', 3),
(N'Hóa học', 3),
(N'Sinh học', 3),
(N'Lịch sử Việt Nam', 4),
(N'Lịch sử thế giới', 4),
(N'Tâm lý học', 5),
(N'Phát triển bản thân', 5),
(N'Kỹ năng mềm', 6),
(N'Kỹ năng cứng', 6),
(N'Truyện tranh', 7),
(N'Sách giáo khoa', 7),
(N'Triết học phương Tây', 8),
(N'Triết học phương Đông', 8),
(N'Y học cơ bản', 9),
(N'Công nghệ thông tin', 10);

-- Insert Publishers
INSERT INTO [Publisher] ([name]) VALUES
(N'NXB Trẻ'),
(N'NXB Kim Đồng'),
(N'NXB Văn học'),
(N'NXB Lao động'),
(N'NXB Thế giới'),
(N'NXB Dân trí'),
(N'NXB Hồng Đức'),
(N'NXB Tổng hợp TPHCM'),
(N'NXB Phụ nữ'),
(N'NXB Thanh niên'),
(N'NXB Khoa học xã hội'),
(N'NXB Giáo dục'),
(N'Alpha Books'),
(N'First News'),
(N'IPM');

-- Insert Books
select * from book

INSERT INTO [Book] ([book_name], [isbn], [cover], [import_price], [first_price], [second_price], [image_url], [author], [description], [year_published], [rating], [pages], [category_id], [publisher_id]) VALUES
(N'Số đỏ', '9786041141524', N'Bìa mềm', 85000, 120000, 95000, 'https://bit.ly/3A1B2C3', N'Vũ Trọng Phụng', N'Tiểu thuyết phê phán hiện thực sâu sắc về xã hội Việt Nam thời kỳ đầu thế kỷ 20', 1936, 4.5, 320, 1, 3),
(N'Tắt đèn', '9786041141531', N'Bìa cứng', 90000, 135000, 108000, 'https://bit.ly/3D4E5F6', N'Ngô Tất Tố', N'Tác phẩm kinh điển về cuộc sống nông thôn Việt Nam', 1937, 4.7, 280, 1, 3),
(N'Chí Phèo', '9786041141548', N'Bìa mềm', 65000, 95000, 76000, 'https://bit.ly/3G7H8I9', N'Nam Cao', N'Truyện ngắn nổi tiếng về số phận con người', 1941, 4.6, 180, 2, 3),
(N'Vợ nhặt', '9786041141555', N'Bìa mềm', 55000, 85000, 68000, 'https://bit.ly/3J0K1L2', N'Kim Lân', N'Truyện ngắn cảm động về tình người thời chiến', 1954, 4.8, 150, 2, 3),
(N'Tôi thấy hoa vàng trên cỏ xanh', '9786041141562', N'Bìa mềm', 120000, 165000, 132000, 'https://bit.ly/3M3N4O5', N'Nguyễn Nhật Ánh', N'Tiểu thuyết về tuổi thơ đầy hoài niệm', 2010, 4.9, 420, 1, 1),
(N'Mắt biếc', '9786041141579', N'Bìa cứng', 110000, 155000, 124000, 'https://bit.ly/3P6Q7R8', N'Nguyễn Nhật Ánh', N'Câu chuyện tình yêu thuở học trò', 1990, 4.8, 380, 1, 1),
(N'Dế Mèn phiêu lưu ký', '9786041141586', N'Bìa mềm', 95000, 135000, 108000, 'https://bit.ly/3S9T0U1', N'Tô Hoài', N'Truyện thiếu nhi kinh điển Việt Nam', 1941, 4.7, 240, 16, 2),
(N'Cánh đồng bất tận', '9786041141593', N'Bìa mềm', 105000, 145000, 116000, 'https://bit.ly/3V2W3X4', N'Nguyễn Ngọc Tư', N'Tiểu thuyết về miền Tây sông nước', 2005, 4.4, 350, 1, 1),
(N'Những ngọn nến cháy', '9786041141600', N'Bìa mềm', 80000, 115000, 92000, 'https://bit.ly/3Y5Z6A7', N'Nguyễn Minh Châu', N'Tuyển tập truyện ngắn hay nhất', 1962, 4.5, 220, 2, 3),
(N'Đời thừa', '9786041141617', N'Bìa mềm', 75000, 110000, 88000, 'https://bit.ly/3B8C9D0', N'Nam Cao', N'Truyện ngắn về cuộc sống khổ cực', 1943, 4.3, 200, 2, 3),
(N'Marketing 4.0', '9786041141624', N'Bìa mềm', 180000, 249000, 199200, 'https://bit.ly/3E1F2G3', N'Philip Kotler', N'Xu hướng marketing thời đại số', 2017, 4.6, 320, 4, 13),
(N'Đắc nhân tâm', '9786041141631', N'Bìa cứng', 150000, 199000, 159200, 'https://bit.ly/3H4I5J6', N'Dale Carnegie', N'Sách kỹ năng giao tiếp kinh điển', 1936, 4.8, 280, 14, 4),
(N'Tư duy nhanh và chậm', '9786041141648', N'Bìa mềm', 200000, 279000, 223200, 'https://bit.ly/3K7L8M9', N'Daniel Kahneman', N'Nghiên cứu về tâm lý học quyết định', 2011, 4.7, 450, 12, 5),
(N'Nhà giả kim', '9786041141655', N'Bìa mềm', 95000, 135000, 108000, 'https://bit.ly/3N0O1P2', N'Paulo Coelho', N'Tiểu thuyết triết lý nổi tiếng thế giới', 1988, 4.5, 180, 1, 5),
(N'Sapiens', '9786041141662', N'Bìa cứng', 220000, 299000, 239200, 'https://bit.ly/3Q3R4S5', N'Yuval Noah Harari', N'Lịch sử loài người từ thời tiền sử', 2011, 4.9, 500, 11, 5),
(N'Homo Deus', '9786041141679', N'Bìa cứng', 210000, 289000, 231200, 'https://bit.ly/3T6U7V8', N'Yuval Noah Harari', N'Tương lai của loài người', 2015, 4.8, 480, 11, 5),
(N'21 bài học cho thế kỷ 21', '9786041141686', N'Bìa mềm', 190000, 259000, 207200, 'https://bit.ly/3W9X0Y1', N'Yuval Noah Harari', N'Những thách thức của thế kỷ 21', 2018, 4.6, 380, 8, 5),
(N'Khéo ăn nói sẽ có được thiên hạ', '9786041141693', N'Bìa mềm', 120000, 165000, 132000, 'https://bit.ly/3Z2A3B4', N'Trác Nhã', N'Kỹ năng giao tiếp và thuyết phục', 2018, 4.2, 260, 14, 6),
(N'Tuổi trẻ đáng giá bao nhiêu', '9786041141700', N'Bìa mềm', 110000, 155000, 124000, 'https://bit.ly/3C5D6E7', N'Rosie Nguyễn', N'Hướng dẫn phát triển bản thân cho giới trẻ', 2019, 4.4, 280, 13, 1),
(N'Nghĩ giàu và làm giàu', '9786041141717', N'Bìa cứng', 165000, 229000, 183200, 'https://bit.ly/3F8G9H0', N'Napoleon Hill', N'Bí quyết thành công tài chính', 1937, 4.5, 350, 6, 4),
(N'Quản trị học', '9786041141724', N'Bìa mềm', 200000, 275000, 220000, 'https://bit.ly/3I1J2K3', N'Stephen Robbins', N'Giáo trình quản trị doanh nghiệp', 2020, 4.3, 520, 5, 12),
(N'Vật lý đại cương', '9786041141731', N'Bìa cứng', 180000, 249000, 199200, 'https://bit.ly/3L4M5N6', N'David Halliday', N'Sách giáo khoa vật lý cơ bản', 2019, 4.6, 600, 7, 12),
(N'Hóa học hữu cơ', '9786041141748', N'Bìa cứng', 195000, 269000, 215200, 'https://bit.ly/3O7P8Q9', N'Paula Bruice', N'Giáo trình hóa học hữu cơ', 2018, 4.4, 580, 8, 12),
(N'Sinh học phân tử', '9786041141755', N'Bìa cứng', 210000, 289000, 231200, 'https://bit.ly/3R0S1T2', N'Bruce Alberts', N'Sinh học tế bào và phân tử', 2020, 4.7, 650, 9, 12),
(N'Lịch sử Việt Nam', '9786041141762', N'Bìa cứng', 150000, 199000, 159200, 'https://bit.ly/3U3V4W5', N'Trần Trọng Kim', N'Lịch sử dân tộc Việt Nam', 1920, 4.8, 420, 10, 11),
(N'Thế chiến thứ hai', '9786041141779', N'Bìa cứng', 180000, 249000, 199200, 'https://bit.ly/3X6Y7Z8', N'Winston Churchill', N'Hồi ký về Thế chiến thứ hai', 1948, 4.9, 800, 11, 5),
(N'Tâm lý học đại cương', '9786041141786', N'Bìa mềm', 170000, 235000, 188000, 'https://bit.ly/3A9B0C1', N'David G. Myers', N'Giáo trình tâm lý học cơ bản', 2019, 4.5, 480, 12, 12),
(N'7 thói quen hiệu quả', '9786041141793', N'Bìa mềm', 140000, 189000, 151200, 'https://bit.ly/3D2E3F4', N'Stephen Covey', N'Phát triển kỹ năng lãnh đạo', 1989, 4.7, 320, 13, 4),
(N'Dạy con làm giàu', '9786041141800', N'Bìa mềm', 130000, 179000, 143200, 'https://bit.ly/3G5H6I7', N'Robert Kiyosaki', N'Giáo dục tài chính cho trẻ em', 1997, 4.4, 280, 6, 4),
(N'Thám tử lừng danh Conan', '9786041141817', N'Bìa mềm', 25000, 35000, 28000, 'https://bit.ly/3J8K9L0', N'Aoyama Gosho', N'Truyện tranh trinh thám nổi tiếng', 1994, 4.8, 200, 16, 2),
(N'Doraemon', '9786041141824', N'Bìa mềm', 20000, 30000, 24000, 'https://bit.ly/3M1N2O3', N'Fujiko F. Fujio', N'Truyện tranh thiếu nhi kinh điển', 1969, 4.9, 180, 16, 2),
(N'Dragon Ball', '9786041141831', N'Bìa mềm', 30000, 42000, 33600, 'https://bit.ly/3P4Q5R6', N'Akira Toriyama', N'Truyện tranh hành động phiêu lưu', 1984, 4.7, 220, 16, 2),
(N'Naruto', '9786041141848', N'Bìa mềm', 28000, 40000, 32000, 'https://bit.ly/3S7T8U9', N'Masashi Kishimoto', N'Truyện tranh ninja nổi tiếng', 1999, 4.8, 200, 16, 2),
(N'One Piece', '9786041141855', N'Bìa mềm', 30000, 42000, 33600, 'https://bit.ly/3V0W1X2', N'Eiichiro Oda', N'Truyện tranh hải tặc phiêu lưu', 1997, 4.9, 210, 16, 2),
(N'Triết học phương Tây', '9786041141862', N'Bìa cứng', 190000, 259000, 207200, 'https://bit.ly/3Y3Z4A5', N'Bertrand Russell', N'Lịch sử triết học phương Tây', 1945, 4.6, 520, 18, 11),
(N'Đạo đức kinh', '9786041141879', N'Bìa mềm', 85000, 120000, 96000, 'https://bit.ly/3B6C7D8', N'Lão Tử', N'Kinh điển triết học Đạo giáo', -600, 4.8, 180, 19, 11),
(N'Luận ngữ', '9786041141886', N'Bìa mềm', 90000, 129000, 103200, 'https://bit.ly/3E9F0G1', N'Khổng Tử', N'Tư tưởng Nho giáo cổ điển', -500, 4.7, 220, 19, 11),
(N'Y học cơ bản', '9786041141893', N'Bìa cứng', 220000, 299000, 239200, 'https://bit.ly/3H2I3J4', N'Kumar Clark', N'Giáo trình y học lâm sàng', 2020, 4.5, 680, 20, 12),
(N'Cấu trúc dữ liệu và giải thuật', '9786041141900', N'Bìa mềm', 180000, 249000, 199200, 'https://bit.ly/3K5L6M7', N'Thomas Cormen', N'Giáo trình khoa học máy tính', 2009, 4.8, 540, 21, 12),
(N'Học máy cơ bản', '9786041141917', N'Bìa mềm', 200000, 275000, 220000, 'https://bit.ly/3N8O9P0', N'Andrew Ng', N'Nhập môn machine learning', 2019, 4.7, 450, 21, 12),
(N'Lập trình Python', '9786041141924', N'Bìa mềm', 160000, 219000, 175200, 'https://bit.ly/3Q1R2S3', N'Mark Lutz', N'Hướng dẫn lập trình Python', 2020, 4.6, 380, 21, 12),
(N'Cơ sở dữ liệu', '9786041141931', N'Bìa mềm', 170000, 235000, 188000, 'https://bit.ly/3T4U5V6', N'Ramez Elmasri', N'Hệ quản trị cơ sở dữ liệu', 2019, 4.4, 420, 21, 12),
(N'Mạng máy tính', '9786041141948', N'Bìa mềm', 185000, 255000, 204000, 'https://bit.ly/3W7X8Y9', N'Andrew Tanenbaum', N'Nguyên lý mạng máy tính', 2018, 4.5, 480, 21, 12),
(N'Trí tuệ nhân tạo', '9786041141955', N'Bìa cứng', 195000, 269000, 215200, 'https://bit.ly/3Z0A1B2', N'Stuart Russell', N'AI hiện đại và ứng dụng', 2020, 4.8, 520, 21, 12),
(N'Blockchain căn bản', '9786041141962', N'Bìa mềm', 155000, 215000, 172000, 'https://bit.ly/3C3D4E5', N'Andreas Antonopoulos', N'Công nghệ blockchain và ứng dụng', 2017, 4.3, 340, 21, 13),
(N'Kinh tế vi mô', '9786041141979', N'Bìa cứng', 190000, 259000, 207200, 'https://bit.ly/3F6G7H8', N'Gregory Mankiw', N'Nguyên lý kinh tế vi mô', 2020, 4.6, 450, 5, 12),
(N'Kinh tế vĩ mô', '9786041141986', N'Bìa cứng', 185000, 255000, 204000, 'https://bit.ly/3I9J0K1', N'Gregory Mankiw', N'Nguyên lý kinh tế vĩ mô', 2020, 4.5, 430, 1, 12),
(N'Quản trị tài chính', '9786041141993', N'Bìa mềm', 175000, 239000, 191200, 'https://bit.ly/3L2M3N4', N'Ross Westerfield', N'Quản trị tài chính doanh nghiệp', 2019, 4.4, 520, 6, 13),
(N'Đầu tư chứng khoán', '9786041142000', N'Bìa mềm', 165000, 229000, 183200, 'https://bit.ly/3O5P6Q7', N'Benjamin Graham', N'Nhà đầu tư thông minh', 1949, 4.7, 380, 6, 4),
(N'Phân tích kỹ thuật', '9786041142017', N'Bìa mềm', 180000, 249000, 199200, 'https://bit.ly/3R8S9T0', N'John Murphy', N'Phân tích kỹ thuật thị trường tài chính', 1999, 4.5, 420, 6, 13),
(N'Kế toán tài chính', '9786041142024', N'Bìa cứng', 195000, 269000, 215200, 'https://bit.ly/3U1V2W3', N'Jerry Weygandt', N'Nguyên lý kế toán tài chính', 2020, 4.3, 480, 5, 12),
(N'Thống kê kinh doanh', '9786041142031', N'Bìa mềm', 170000, 235000, 188000, 'https://bit.ly/3X4Y5Z6', N'David Anderson', N'Thống kê ứng dụng trong kinh doanh', 2019, 4.4, 390, 5, 12),
(N'Marketing số', '9786041142048', N'Bìa mềm', 160000, 219000, 175200, 'https://bit.ly/3A7B8C9', N'Dave Chaffey', N'Marketing trong thời đại số', 2019, 4.6, 350, 4, 13),
(N'Bán hàng hiệu quả', '9786041142055', N'Bìa mềm', 140000, 189000, 151200, 'https://bit.ly/3D0E1F2', N'Brian Tracy', N'Kỹ thuật bán hàng chuyên nghiệp', 2013, 4.5, 280, 14, 4),
(N'Lãnh đạo nhóm', '9786041142062', N'Bìa mềm', 135000, 185000, 148000, 'https://bit.ly/3G3H4I5', N'John Maxwell', N'Phát triển kỹ năng lãnh đạo', 2007, 4.6, 300, 15, 13),
(N'Quản lý thời gian', '9786041142079', N'Bìa mềm', 125000, 175000, 140000, 'https://bit.ly/3J6K7L8', N'Brian Tracy', N'Kỹ năng quản lý thời gian hiệu quả', 2007, 4.4, 240, 15, 13),
(N'Thuyết trình hiệu quả', '9786041142086', N'Bìa mềm', 130000, 179000, 143200, 'https://bit.ly/3M9N0O1', N'Dale Carnegie', N'Kỹ năng thuyết trình và giao tiếp', 1956, 4.7, 260, 14, 4),
(N'Tư duy sáng tạo', '9786041142093', N'Bìa mềm', 145000, 199000, 159200, 'https://bit.ly/3P2Q3R4', N'Edward de Bono', N'Phát triển tư duy sáng tạo', 1985, 4.5, 320, 13, 5),
(N'Đàm phán thành công', '9786041142100', N'Bìa mềm', 155000, 215000, 172000, 'https://bit.ly/3S5T6U7', N'Roger Fisher', N'Nghệ thuật đàm phán hiệu quả', 1981, 4.6, 280, 14, 5),
(N'Emotional Intelligence', '9786041142117', N'Bìa mềm', 165000, 229000, 183200, 'https://bit.ly/3V8W9X0', N'Daniel Goleman', N'Trí tuệ cảm xúc trong công việc', 1995, 4.8, 350, 12, 5),
(N'Mindset', '9786041142124', N'Bìa mềm', 150000, 199000, 159200, 'https://bit.ly/3Y1Z2A3', N'Carol Dweck', N'Tâm lý học thành công', 2006, 4.7, 290, 12, 5),
(N'Flow', '9786041142131', N'Bìa mềm', 160000, 219000, 175200, 'https://bit.ly/3B4C5D6', N'Mihaly Csikszentmihalyi', N'Tâm lý học của trải nghiệm tối ưu', 1990, 4.6, 320, 12, 5),
(N'Grit', '9786041142148', N'Bìa mềm', 155000, 215000, 172000, 'https://bit.ly/3E7F8G9', N'Angela Duckworth', N'Sức mạnh của đam mê và kiên trì', 2016, 4.5, 310, 13, 5),
(N'Atomic Habits', '9786041142155', N'Bìa mềm', 170000, 235000, 188000, 'https://bit.ly/3H0I1J2', N'James Clear', N'Thay đổi tí hon hiệu quả bất ngờ', 2018, 4.9, 320, 13, 5),
(N'The Power of Now', '9786041142162', N'Bìa mềm', 145000, 199000, 159200, 'https://bit.ly/3K3L4M5', N'Eckhart Tolle', N'Sức mạnh của hiện tại', 1997, 4.4, 280, 8, 5),
(N'Man Search for Meaning', '9786041142179', N'Bìa mềm', 140000, 189000, 151200, 'https://bit.ly/3N6O7P8', N'Viktor Frankl', N'Tìm kiếm ý nghĩa cuộc sống', 1946, 4.8, 250, 8, 5);


update book set image_url = 'https://sachcuabc.com/wp-content/uploads/2021/03/Kafka-ben-bo-bien.jpg'

update book set quantity = 99;

alter table BookRating add constraint fk_br_u foreign key (user_id) references AspNetUsers(id)
alter table BookComment add constraint fk_bc_u foreign key (user_id) references AspNetUsers(id)
alter table [Order] add constraint fk_o_u foreign key (user_id) references AspNetUsers(id)

INSERT INTO AspNetRoles(Name, NormalizedName) Values ('User', 'USER');
INSERT INTO AspNetRoles(Name, NormalizedName) Values ('Admin', 'ADMIN');

INSERT INTO VOUCHER(voucher_code, type, create_at, expired_at, usage_limit, used_count, value) 
VALUES ('30kTONGTIEN', 0, GETDATE(), '2030-12-31', 100000, 0, 30000);
INSERT INTO VOUCHER(voucher_code, type, create_at, expired_at, usage_limit, used_count, value) 
VALUES ('10%TONGTIEN', 1, GETDATE(), '2030-12-31', 100000, 0, 0.1);

INSERT INTO PAYMENTTYPE(type) VALUES ('COD');
INSERT INTO PAYMENTTYPE(type) VALUES ('BANKING');

select * from Voucher
select * from [Order]
select * from Cart


Update [Order] Set status = 'CANCELLED'
