# 介绍
这是一个表结构管理并自动生成代码的开发辅助工具。

在开发中，很多代码都是由表结构而相应编写的，如 Java 中的类、Mapper，C++的结构体等。这部分代码与表结构是对应的，具有固定的模式。因此开发一个通用的工具，能根据表结构生成任意类型的代码，使开发人员最大限度减少编码工作量。

本工具是基于本仓库的客户端通用框架项目 UniClient 开发而成。

# 使用

### 多系统管理

本工具能同时管理多个系统的表结构资料。在打开后的初始界面输入系统名称并进入，工具将自动创建该系统的资料文件夹。也可在下拉列表中选择或删除已创建的系统资料。不同系统的资料相互隔离。

### 字段管理

首先在字段管理页面定义字段。本工具是先定义字段，再在表中引用字段而定义表结构的。这样做的好处在于：

（1）保证不同表的同名字段具有相同的含意和类型。对于名字相同的字段，C++ 程序中可以放心地使用 strcpy 方法而无需做额外的保护；

（2）若需要变更字段类型，只需维护一次而所有表都生效，无需个各表逐一操作。

本工具定义的字段类型及与各程序语言、Database 的对应关系如下。

| 类型 | 用途 | C++ | C# | Java | MySQL | SQL Server | Oracle | Thrift | gRPC |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Int32 | 32位整型 | int32_t | int | Integer | INT | INT | NUMBER(10) | i32 | int32 |
| Int64 | 64位整型 | int64_t | long | Long | BIGINT | BIGINT | NUMBER(19) | i64 | int64 |
| Number(m,n) | 浮点数值 | double | double | Double | DECIMAL(m,n) | DECIMAL(m,n) | NUMBER(m,n) | double | double |
| Char(m), n | 定长字串 | std::string
char[m * n + 1] | string
byte[m * n + 1] | String
Byte[m * n + 1] | CHAR(m) | CHAR(m) | CHAR(m) | string | string |
| Varchar(m), n | 变长字串 | std::string
char[m * n + 1] | string
byte[m * n + 1] | String
Byte[m * n + 1] | VARCHAR(m) | VARCHAR(m) | VARCHAR2(m) | string | string |
| Bool | 真假值 | bool | bool | Boolean | TINYINT | BIT | Number(1) | bool | bool |
| Datetime | 时间 | std::chrono | (System.)DateTime | (java.time.)LocalDateTime | DATETIME | DATETIME | DATE | string | string |

**说明**

（1）Number(m,n)，m 为精度，n 为小数位数；

（2）Char、Varchar，m 为字符个数，n 为单字符所占最大字节数。n 的值须根据实际使用场景确定，如编码为UTF-8，而字段实际只会存放英文字符，则 n = 1。m 和 n 的数值影响对应的程序中字符数组的长度；

（3）工具中精度和字符个数共用一列展示，小数位数和单字符所占最大字节数共用一列展示。

### 表管理

在表菜单下进行表的管理。工具考虑到了可能有多数据库的情况，首先添加数据库，在数据库下添加表。在表菜单中，可设置表的字段、索引等。

**注意** 在字段、表的管理菜单中，增、删、改数据后需要保存方生效。

字段和表的数据是以 xml 文件的形式保存在 userdata 目录下。

# 代码生成

### 生成任务

为达到通用的目的，代码生成的任务是由用户自行配置的，配置方法如下。

在 **userdata\对应系统\templates** 的目录下新增文件夹，文件夹名称对应生成任务的名称。在文件夹下添加 GenTask.xml 生成任务配置文件，该配置文件中设置以下内容：

- **TemplateFile**：模板文件名称，以 .template 扩展名结尾；若以 .cs 扩展名结尾，则工具将其作为 C# 脚本进行执行，并将已设置的字段、表数据传入，此功能尚待验证；
- **RecursionLevel**：迭代层级，可选择 Database 或 Table；对于迭代层级为 Database 的生成任务，工具将从数据库层级开始，逐一按模板生成代码，每个 database 对应一个生成文件；对于迭代层级为 Table 的生成任务，工具将从表的层级开始，每张表生成一个代码文件，并分数据库目录存放（若只有一个 Database 则直接存放至生成路径下）；
- **OutputFile**：输出文件名称；
- **ProgramLanguage**：编程语言，可选择Cpp、CSharp、Java，影响对应的程序字段类型；
- **IsUsingString**：是否使用String，是则 Char、Varchar 类型字段对应的程序字段类型为 String，否则为 byte 数组；
- **DatabaseType**：数据库类型，影响对应的数据库字段类型。

### 生成模板

工具以模板文件，按照维护的表数据生成实际的代码文件，而模板文件中包含以各类宏定义，使得用户能灵活地定制生成的代码文件。

模板文件中的宏可分为全局类宏和迭代类宏，以模板生成代码的规则可概括为：若行中不包含宏，则以原文复制到生成文件中；若行中包含全局类宏，则将宏替换为对应值后，输出到生成文件中；若行中包含迭代类宏，则将对应的对象列表依次替换宏的值，并依次输出到生成文件中。

不同迭代层级中，如以数据库和以表迭代时，对应的全局类宏和迭代类宏各不相同。迭代类宏有分组，在使用迭代类宏时，必须在一行内使用同一组的宏，也必须将一组宏写在同一行内。若一行内使用了多组迭代类宏，仅会处理第一个匹配的宏，其余的宏不会替换，会原样在代码文件输出，导致结果不正确；一组宏写在同一行内，若需要生成的代码产生换行，需使用全局类宏 NEWLINE。

所有宏的书写方式可按照 **${宏名称(命名风格)}** 格式。各迭代层级的宏见后文；命名风格包含以下选项：

- lowerCamel：首字母小写驼峰命名；
- UpperCamel：首字母大写驼峰命名；
- lower_snake：小写下划线蛇形命名；
- UPPER_SNAKE：大写下划线蛇形命名；
- all_lower：全小写；
- ALL_UPPER：全大写。

其中命名风格也可省略，则工具会按照原样的表名、字段名等替代。命名风格的支持，使得可以在工具内以任意统一的命名风格维护表和字段，而在通过模板生成另一个命名风格的代码。

# 宏

### 通用宏

- NEWLINE：换行；
- BACKSPACE：退格；
- CONDITIONAL_SPACE：若前、后方字符都不为空格，则插入空格；若前后有多个空格，也将合并为一个空格；
- TEMPLATE：子模板，该宏的书写规范为 ${TEMPLATE(模板文件名称,迭代层级)}。

### 数据库迭代

全局类宏：

- DatabaseName：数据库名称；
- DatabaseDescription：数据库描述。

### 表迭代

全局类宏：

- DatabaseName：数据库名称；
- TableName：表名称；
- TableDescription：表描述；
- PrimaryKeyName：主键名称；
- PrimaryKeyColumnCount：主键字段个数；
- PrimaryKeyColumns：以逗点隔开的主键字段名称列表；
- PrimaryKeyColumnsWithBackQuota：以逗点隔开的主键字段名称列表，各名称前后加反引号；

迭代类宏：

（1）字段迭代

- ColumnName：字段名称；
- ColumnDescription：字段描述；
- ColumnDbType：字段数据库类型，其值与生成任务的 DatabaseType 设置有关，并按照 **字段管理** 一节的对应关系确定其值；
- ColumnDbDefaultString：字段缺省字串，对于无缺省值的字段为空串，对于有缺省值的字段为 "default"，该宏可用于建表语句；
- ColumnDbDefaultValue：数据库缺省值，对于无缺省值的字段为空串，对于有缺省值的字段，字串类字段将在前后加单引号，该宏可用于建表语句；
- ColumnDbNullableFlag：字段可空标志，对于可为 null 的字段为空串，对于不可为空的字段为 "not null"，该宏可用于建表语句；
- ColumnProgramType：字段对应程式数据类型，其值与生成任务的 ProgramLanguage 设置有关，并按照 **字段管理** 一节的对应关系确定其值；
- ColumnHungarianPrefix：匈牙利命名法前缀，Int32: n，Int64：l，Number：d，Char、Varchar：sz，Bool：b，Datetime：dt；
- ColumnComma：逗点，迭代时最后一个字段为空串，其余为一逗号；

（2）普通字段（非主键字段）迭代

- GeneralColumnName：字段名称；
- GeneralColumnDescription：字段描述；
- GeneralColumnDbType：字段数据库类型；
- GeneralColumnDbDefaultString：字段缺省字串；
- GeneralColumnDbDefaultValue：数据库缺省值；
- GeneralColumnDbNullableFlag：字段可空标志；
- GeneralColumnProgramType：字段对应程式数据类型；
- GeneralColumnHungarianPrefix：匈牙利命名法前缀；
- GeneralColumnComma：逗点；

(3) 主键字段迭代

- PrimaryKeyColumnName：字段名称；
- PrimaryKeyColumnDescription：字段描述；
- PrimaryKeyColumnDbType：：字段数据库类型；
- PrimaryKeyColumnDbDefaultString：字段缺省字串；
- PrimaryKeyColumnDbDefaultValue：数据库缺省值；
- PrimaryKeyColumnDbNullableFlag：字段可空标志；
- PrimaryKeyColumnProgramType：字段对应程式数据类型；
- PrimaryKeyColumnHungarianPrefix：匈牙利命名法前缀；
- PrimaryKeyColumnComma：逗点；
- PrimaryKeyColumnIndex：字段索引下标值；
- PrimaryKeyColumnAutoIncrement：被设置为自增的字段，值为“auto_increment”，其他字段则为空串；

（4）索引迭代（包含 UNIQUE 和 INDEX 类索引）

- IndexType：索引类型，唯一索引的值为 "unique"，非唯一索引的值为 "index"；
- IndexTypeWithKey：带有Key关键字段索引类型，唯一索引的值为 "unique key"，非唯一索引的值为 "index"；
- IndexName：索引名称；
- IndexColumns：以逗点隔开的索引字段名称列表；
- IndexColumnsWithBackQuota：以逗点隔开的索引字段名称列表，各名称前后加反引号；

（5）唯一索引迭代

- UniqueIndexName：索引名称；
- UniqueIndexColumns：以逗点隔开的索引字段名称列表；
- UniqueIndexColumnsWithBackQuota：以逗点隔开的索引字段名称列表，各名称前后加反引号；

（6）非唯一索引迭代

- NonUniqueIndexName：索引名称；
- NonUniqueIndexColumns：以逗点隔开的索引字段名称列表；
- NonUniqueIndexColumnsWithBackQuota：以逗点隔开的索引字段名称列表，各名称前后加反引号；

（7）外键迭代

- ForeignKeyName：外键名称；
- ForeignKeyColumnName：外键字段；
- ForeignKeyReferenceTableName：外键关联表名称；
- ForeignKeyReferenceColumnName：外键关联字段名称；

（8）自增字段迭代

- AutoIncColumnName：字段名称；
- AutoIncColumnDescription：字段描述；
- AutoIncColumnDbType：字段数据库类型；
- AutoIncColumnDbDefaultString：字段缺省字串；
- AutoIncColumnDbDefaultValue：数据库缺省值；
- AutoIncColumnDbNullableFlag：字段可空标志；
- AutoIncColumnProgramType：字段对应程式数据类型；
- AutoIncColumnHungarianPrefix：匈牙利命名法前缀。

# 示例

在工具初始的包中，有预设的 HRSystem，其中包含了 tdept 和 temployee 两张表。预设示例演示了如何用本工具生成 C++ 结构体、Java 类、MyBatis 的 Mapper、MySQL 和 Oracle 的初始化建表脚本和表结构升级脚本。

在 MySQL 和 Oracle 的初始化建表脚本中，预设了一个名为 upgrade_table 的储存过程，在该储存过程中读取由外部传入的建表语句，建立临时表并于当前表比较字段、索引是否有差异。若字段有差异则将当前表数据导入临时表中，并在数据导入完成后以临时表替换当前表；若索引有差异则重新建立索引；若无任何差异则不执行任何操作。在表结构升级脚本中，工具根据表结构数据生成调用该储存过程的 SQL 语句。正是因为有该工具的支持，使得在工具内维护表结构数据后，执行升级脚本能保证最终的表结构与工具内部定义一致。
