# 介紹
這是一個表結構管理並自動生成代碼的開發輔助工具。

在開發中，很多代碼都是由表結構而相應編寫的，如 Java 中的類、Mapper，C++的結構體等。這部分代碼與表結構是對應的，具有固定的模式。因此開發一個通用的工具，能根據表結構生成任意類型的代碼，使開發人員最大限度減少編碼工作量。

本工具是基於本倉庫的客戶端通用框架項目 UniClient 開發而成。

# 使用

### 多系統管理

本工具能同時管理多個系統的表結構資料。在打開後的初始界面輸入系統名稱並進入，工具將自動創建該系統的資料文件夾。也可在下拉列表中選擇或刪除已創建的系統資料。不同系統的資料相互隔離。

### 字段管理

首先在字段管理頁面定義字段。本工具是先定義字段，再在表中引用字段而定義表結構的。這樣做的好處在於：

（1）保證不同表的同名字段具有相同的含意和類型。對於名字相同的字段，C++ 程序中可以放心地使用 strcpy 方法而無需做額外的保護；

（2）若需要變更字段類型，只需維護一次而所有表都生效，無需個各表逐一操作。

本工具定義的字段類型及與各程序語言、Database 的對應關係如下。

| 類型 | 用途 | C++ | C# | Java | MySQL | SQL Server | Oracle | Thrift | gRPC |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Int32 | 32位整型 | int32_t | int | Integer | INT | INT | NUMBER(10) | i32 | int32 |
| Int64 |  64位整型 | int64_t | long | Long | BIGINT | BIGINT | NUMBER(19) | i64 | int64 |
| Number(m,n) | 浮點數值 | double | double | Double | DECIMAL(m,n) | DECIMAL(m,n) | NUMBER(m,n) | double | double |
| Char(m), n | 定長字串 | std::string<br>char[m * n + 1] | string<br>byte[m * n + 1] | String<br>Byte[m * n + 1] | CHAR(m) | CHAR(m) | CHAR(m) | string | string |
| Varchar(m), n | 變長字串 | std::string<br>char[m * n + 1] | string<br>byte[m * n + 1] | String<br>Byte[m * n + 1] | VARCHAR(m) | VARCHAR(m) | VARCHAR2(m) | string | string |
| Bool | 真假值 | bool | bool | Boolean | TINYINT | BIT | Number(1) | bool | bool |
| Datetime | 時間 | std::chrono | (System.)DateTime | (java.time.)LocalDateTime | DATETIME | DATETIME | DATE | string | string |

**說明**

（1）Number(m,n)，m 為精度，n 為小數位數；

（2）Char、Varchar，m 為字符個數，n 為單字符所占最大字節數。n 的值須根據實際使用場景確定，如編碼為UTF-8，而字段實際只會存放英文字符，則 n = 1。m 和 n 的數值影響對應的程序中字符數組的長度；

（3）工具中精度和字符個數共用一列展示，小數位數和單字符所占最大字節數共用一列展示。

### 表管理

在表菜單下進行表的管理。工具考慮到了可能有多資料庫的情況，首先添加資料庫，在資料庫下添加表。在表菜單中，可設置表的字段、索引等。

**注意** 在字段、表的管理菜單中，增、刪、改數據後需要保存方生效。

字段和表的數據是以 xml 文件的形式保存在 userdata 目錄下。

# 代碼生成

### 生成任務

為達到通用的目的，代碼生成的任務是由用戶自行配置的，配置方法如下。

在 **userdata\對應系統\templates** 的目錄下新增文件夾，文件夾名稱對應生成任務的名稱。在文件夾下添加 GenTask.xml 生成任務配置文件，該配置文件中設置以下內容：

- **TemplateFile**：模板文件名稱，以 .template 擴展名結尾；若以 .cs 擴展名結尾，則工具將其作為 C# 腳本進行執行，並將已設置的字段、表數據傳入，此功能尚待驗證；
- **RecursionLevel**：迭代層級，可選擇 Database 或 Table；對於迭代層級為 Database 的生成任務，工具將從資料庫層級開始，逐一按模板生成代碼，每個 database 對應一個生成文件；對於迭代層級為 Table 的生成任務，工具將從表的層級開始，每張表生成一個代碼文件，並分資料庫目錄存放（若只有一個 Database 則直接存放至生成路徑下）；
- **OutputFile**：輸出文件名稱；
- **ProgramLanguage**：編程語言，可選擇Cpp、CSharp、Java，影響對應的程序字段類型；
- **IsUsingString**：是否使用String，是則 Char、Varchar 類型字段對應的程序字段類型為 String，否則為 byte 數組；
- **DatabaseType**：資料庫類型，影響對應的資料庫字段類型。

### 生成模板

工具以模板文件，按照維護的表數據生成實際的代碼文件，而模板文件中包含以各類宏定義，使得用戶能靈活地定製生成的代碼文件。

模板文件中的宏可分為全局類宏和迭代類宏，以模板生成代碼的規則可概括為：若行中不包含宏，則以原文複製到生成文件中；若行中包含全局類宏，則將宏替換為對應值後，輸出到生成文件中；若行中包含迭代類宏，則將對應的對象列表依次替換宏的值，並依次輸出到生成文件中。

不同迭代層級中，如以資料庫和以表迭代時，對應的全局類宏和迭代類宏各不相同。迭代類宏有分組，在使用迭代類宏時，必須在一行內使用同一組的宏，也必須將一組宏寫在同一行內。若一行內使用了多組迭代類宏，僅會處理第一個匹配的宏，其餘的宏不會替換，會原樣在代碼文件輸出，導致結果不正確；一組宏寫在同一行內，若需要生成的代碼產生換行，需使用全局類宏 NEWLINE。

所有宏的書寫方式可按照 **${宏名稱(命名風格)}** 格式。各迭代層級的宏見後文；命名風格包含以下選項：

- lowerCamel：首字母小寫駝峰命名；
- UpperCamel：首字母大寫駝峰命名；
- lower_snake：小寫下劃綫蛇形命名；
- UPPER_SNAKE：大寫下劃綫蛇形命名；
- all_lower：全小寫；
- ALL_UPPER：全大寫。

其中命名風格也可省略，則工具會按照原樣的表名、字段名等替代。命名風格的支持，使得可以在工具內以任意統一的命名風格維護表和字段，而在通過模板生成另一個命名風格的代碼。

# 宏

### 通用宏

- NEWLINE：換行；
- BACKSPACE：退格；
- CONDITIONAL_SPACE：若前、後方字符都不為空格，則插入空格；若前後有多個空格，也將合併為一個空格；
- TEMPLATE：子模板，該宏的書寫規範為 ${TEMPLATE(模板文件名稱,迭代層級)}。

### 資料庫迭代

全局類宏：

- DatabaseName：資料庫名稱；
- DatabaseDescription：資料庫描述。

### 表迭代

全局類宏：

- DatabaseName：資料庫名稱；
- TableName：表名稱；
- TableDescription：表描述；
- PrimaryKeyName：主鍵名稱；
- PrimaryKeyColumnCount：主鍵字段個數；
- PrimaryKeyColumns：以逗點隔開的主鍵字段名稱列表；
- PrimaryKeyColumnsWithBackQuota：以逗點隔開的主鍵字段名稱列表，各名稱前後加反引號；

迭代類宏：

（1）字段迭代

- ColumnName：字段名稱；
- ColumnDescription：字段描述；
- ColumnDbType：字段資料庫類型，其值與生成任務的 DatabaseType 設置有關，並按照 **字段管理** 一節的對應關係確定其值；
- ColumnDbDefaultString：字段缺省字串，對於無缺省值的字段為空串，對於有缺省值的字段為 "default"，該宏可用於建表語句；
- ColumnDbDefaultValue：資料庫缺省值，對於無缺省值的字段為空串，對於有缺省值的字段，字串類字段將在前後加單引號，該宏可用於建表語句；
- ColumnDbNullableFlag：字段可空標誌，對於可為 null 的字段為空串，對於不可為空的字段為 "not null"，該宏可用於建表語句；
- ColumnProgramType：字段對應程式數據類型，其值與生成任務的 ProgramLanguage 設置有關，並按照 **字段管理** 一節的對應關係確定其值；
- ColumnComma：逗點，迭代時最後一個字段為空串，其餘為一逗號；

（2）普通字段（非主鍵字段）迭代

- GeneralColumnName：字段名稱；
- GeneralColumnDescription：字段描述；
- GeneralColumnDbType：字段資料庫類型；
- GeneralColumnDbDefaultString：字段缺省字串；
- GeneralColumnDbDefaultValue：資料庫缺省值；
- GeneralColumnDbNullableFlag：字段可空標誌；
- GeneralColumnProgramType：字段對應程式數據類型；
- GeneralColumnComma：逗點；

(3) 主鍵字段迭代

- PrimaryKeyColumnName：字段名稱；
- PrimaryKeyColumnDescription：字段描述；
- PrimaryKeyColumnDbType：：字段資料庫類型；
- PrimaryKeyColumnDbDefaultString：字段缺省字串；
- PrimaryKeyColumnDbDefaultValue：資料庫缺省值；
- PrimaryKeyColumnDbNullableFlag：字段可空標誌；
- PrimaryKeyColumnProgramType：字段對應程式數據類型；
- PrimaryKeyColumnComma：逗點；
- PrimaryKeyColumnIndex：字段索引下標值；
- PrimaryKeyColumnAutoIncrement：被設置為自增的字段，值為“auto_increment”，其他字段則為空串；

（4）索引迭代（包含 UNIQUE 和 INDEX 類索引）

- IndexType：索引類型，唯一索引的值為 "unique"，非唯一索引的值為 "index"；
- IndexTypeWithKey：帶有Key關鍵字段索引類型，唯一索引的值為 "unique key"，非唯一索引的值為 "index"；
- IndexName：索引名稱；
- IndexColumns：以逗點隔開的索引字段名稱列表；
- IndexColumnsWithBackQuota：以逗點隔開的索引字段名稱列表，各名稱前後加反引號；

（5）唯一索引迭代

- UniqueIndexName：索引名稱；
- UniqueIndexColumns：以逗點隔開的索引字段名稱列表；
- UniqueIndexColumnsWithBackQuota：以逗點隔開的索引字段名稱列表，各名稱前後加反引號；

（6）非唯一索引迭代

- NonUniqueIndexName：索引名稱；
- NonUniqueIndexColumns：以逗點隔開的索引字段名稱列表；
- NonUniqueIndexColumnsWithBackQuota：以逗點隔開的索引字段名稱列表，各名稱前後加反引號；

（7）外鍵迭代

- ForeignKeyName：外鍵名稱；
- ForeignKeyColumnName：外鍵字段；
- ForeignKeyReferenceTableName：外鍵關聯表名稱；
- ForeignKeyReferenceColumnName：外鍵關聯字段名稱；

（8）自增字段迭代

- AutoIncColumnName：字段名稱；
- AutoIncColumnDescription：字段描述；
- AutoIncColumnDbType：字段資料庫類型；
- AutoIncColumnDbDefaultString：字段缺省字串；
- AutoIncColumnDbDefaultValue：資料庫缺省值；
- AutoIncColumnDbNullableFlag：字段可空標誌；
- AutoIncColumnProgramType：字段對應程式數據類型。
