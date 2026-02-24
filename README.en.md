# Introduction
This is a development aid tool for managing table structures and automatically generating code.

In development, much code is written based on table structures, such as classes and Mappers in Java, and structs in C++. This code corresponds to the table structure and has a fixed pattern. Therefore, developing a general-purpose tool that can generate any type of code based on the table structure minimizes the coding workload for developers.

This tool is developed based on the UniClient project, a general-purpose client framework in this repository.

# Usage

### Multi-System Management

This tool can manage table structure data from multiple systems simultaneously. Enter the system name in the initial interface after opening, and the tool will automatically create a data folder for that system. You can also select or delete created system data from the drop-down list. Data from different systems is isolated from each other.

### Field Management

First, define the fields on the field management page. This tool defines the fields first, and then references the fields in the table to define the table structure. The advantages of doing this are:

(1) Ensuring that fields with the same name in different tables have the same meaning and type. For fields with the same name, the `strcpy` method can be used safely in C++ programs without additional protection;

(2) If a field type needs to be changed, it only needs to be maintained once and all tables will be affected, without needing to operate on each table individually.

The field types defined in this tool and their correspondence with various programming languages ​​and databases are as follows.

| Type | Purpose | C++ | C# | Java | MySQL | SQL Server | Oracle | Thrift | gRPC |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Int32 | 32-bit integer | int32_t | int | Integer | INT | INT | NUMBER(10) | i32 | int32 |
| Int64 | 64-bit integer | int64_t | long | Long | BIGINT | BIGINT | NUMBER(19) | i64 | int64 |
| Number(m,n) | Floating-point number | double | double | Double | DECIMAL(m,n) | DECIMAL(m,n) | NUMBER(m,n) | double | double |
| Char(m), n | fixed-length string | std::string<br>char[m * n + 1] | string<br>byte[m * n + 1] | String<br>Byte[m * n + 1] | CHAR(m) | CHAR(m) | CHAR(m) | string | string |
| Varchar(m), n | Variable length string | std::string<br>char[m * n + 1] | string<br>byte[m * n + 1] | String<br>Byte[m * n + 1] | VARCHAR(m) | VARCHAR(m) | VARCHAR2(m) | string | string |
| Bool | True or false value | bool | bool | Boolean | TINYINT | BIT | Number(1) | bool | bool |
| Datetime | Time | std::chrono | (System.)DateTime | (java.time.)LocalDateTime | DATETIME | DATETIME | DATE | string | string |

**Explanation**

(1) Number(m,n), where m is the precision and n is the number of decimal places.

(2) Char, Varchar, where m is the number of characters and n is the maximum number of bytes per character. The value of n must be determined based on the actual usage scenario. For example, if the encoding is UTF-8 but the field only stores English characters, then n = 1. The values ​​of m and n affect the length of the character array in the corresponding program.

(3) In the tool, precision and number of characters are displayed in one column, while the number of decimal places and the maximum number of bytes per character are displayed in another column.

### Table Management

Manage tables under the Table menu. The tool considers the possibility of multiple databases. First, add a database, then add tables under the database. In the Table menu, you can set the table's fields, indexes, etc.

**Note** In the field and table management menus, you need to save the changes after adding, deleting, or modifying data for them to take effect.

The data for the fields and tables is stored in the userdata directory as XML files.

# Code Generation

### Generation Task

For general purposes, the code generation task is configured by the user. The configuration method is as follows:

Create a new folder in the directory **userdata\corresponding system\templates**, and name the folder the corresponding task name. Add a GenTask.xml configuration file to the folder to generate the task. This configuration file should contain the following settings:

- **TemplateFile**: Template file name, ending with the .template extension; if ending with .cs, the tool will execute it as a C# script, passing in the pre-defined fields and table data. Please refer to the table structure document generation demo.

- **RecursionLevel**: Iteration level, selectable between Database and Table. For generation tasks with a Database iteration level, the tool will start from the database level and generate code sequentially according to the template, with one generation file for each database. For generation tasks with a Table iteration level, the tool will start from the table level, generating one code file for each table and storing them in separate database directories (if there is only one Database, it will be stored directly in the generation path).

- **OutputFile**: Output file name.

- **ProgramLanguage**: Programming language, selectable between Cpp, CSharp, and Java, affecting the corresponding program field types.

- **IsUsingString**: Whether to use String; if yes, Char or Varchar. The type field corresponds to a String field in the program; otherwise, it's a byte array.

- **DatabaseType**: Database type, affecting the corresponding database field type.

### Generating Templates

The tool generates actual code files based on template files and the maintained table data. The template files contain various macro definitions, allowing users to flexibly customize the generated code files.

Macros in the template files can be divided into global macros and iterative macros. The rules for generating code from templates can be summarized as follows: if a line does not contain a macro, it is copied verbatim to the generated file; if a line contains a global macro, the macro is replaced with its corresponding value and output to the generated file; if a line contains iterative macros, the corresponding list of objects is used to replace the macro values ​​sequentially and output sequentially to the generated file.

The corresponding global macros and iterative macros differ at different iteration levels, such as when iterating by database and table. Iterative macros are grouped; when using iterative macros, macros from the same group must be used on the same line, and a group of macros must be written on the same line. If multiple iterative macros are used within a single line, only the first matching macro will be processed. The remaining macros will not be replaced and will be output as is in the code file, leading to incorrect results. If a group of macros is written on the same line and a newline is required in the generated code, the global macro `NEWLINE` must be used.

All macros should be written in the format `**${macro name(naming style)}**`. Macros at each iteration level are listed below. Naming styles include the following options:

- `lowerCamel`: Camel Case with the first letter lowercase;
- `UpperCamel`: Camel Case with the first letter uppercase;
- `lower_snake`: Snake-like naming with lowercase underscores;
- `UPPER_SNAKE`: Snake-like naming with uppercase underscores;
- `all_lower`: All lowercase;
- `ALL_UPPER`: All uppercase.

The naming style can also be omitted, in which case the tool will replace the table names, field names, etc., as is. Support for naming styles allows tables and fields to be maintained within the tool using any uniform naming style, while code with a different naming style is generated from templates.

# Macros

### General Macros

- NEWLINE: Newline;
- BACKSPACE: Backspace;
- CONDITIONAL_SPACE: Insert a space if neither the preceding nor following characters are spaces; if there are multiple spaces before and after, merge them into one space;
- TEMPLATE: Sub-template. The syntax for this macro is ${TEMPLATE(template file name, iteration level)}.

### Database Iteration

Global Macros:

- DatabaseName: Database name;
- DatabaseDescription: Database description.

### Table Iteration

Global Macros:

- DatabaseName: Database name;
- TableName: Table name;
- TableDescription: Table description;
- PrimaryKeyName: Primary key name;
- PrimaryKeyColumnCount: Number of primary key fields;
- PrimaryKeyColumns: A comma-separated list of primary key field names;
- PrimaryKeyColumnsWithBackQuota: A comma-separated list of primary key field names, with backticks before and after each name;

Iteration Macros:

(1) Field Iteration

- ColumnName: Field name;
- ColumnDescription: Field description;
- ColumnDbType: Database type of the field. Its value is related to the DatabaseType setting of the generated task and is determined according to the correspondence in the **Field Management** section;
- ColumnDbDefaultString: Default string for the field. For fields without default values, it is an empty string; for fields with default values, it is "default". This macro can be used in table creation statements;
- ColumnDbDefaultValue: Database default value. For fields without default values, it is an empty string. For fields with default values, string fields will be enclosed in single quotes before and after. This macro can be used in table creation statements;
- ColumnDbNullableFlag: Field nullability flag. For fields that can be null, it is an empty string. For fields that cannot be null, it is "not null". This macro can be used in table creation statements;
- ColumnProgramType: Field's corresponding program data type. Its value is related to the ProgramLanguage setting of the generated task and is determined according to the correspondence in the **Field Management** section;
- ColumnHungarianPrefix: Hungarian notation prefix. Int32: n, Int64: l, Number: d, Char, Varchar: sz, Bool: b, Datetime: dt;
- ColumnComma: Comma. During iteration, the last field is an empty string, and the rest are commas;

(2) Iteration of ordinary fields (non-primary key fields)

- GeneralColumnName: Field name;
- GeneralColumnDescription: Field description;
- GeneralColumnDbType: Field database type;
- GeneralColumnDbDefaultString: Field default string;
- GeneralColumnDbDefaultValue: Database default value;
- GeneralColumnDbNullableFlag: Field nullable flag;
- GeneralColumnProgramType: Field corresponding program data type;
- GeneralColumnHungarianPrefix: Hungarian notation prefix;
- GeneralColumnComma: Comma;

(3) Primary Key Field Iteration

- PrimaryKeyColumnName: Field name;
- PrimaryKeyColumnDescription: Field description;
- PrimaryKeyColumnDbType: Field database type;
- PrimaryKeyColumnDbDefaultString: Field default string;
- PrimaryKeyColumnDbDefaultValue: Database default value;
- PrimaryKeyColumnDbNullableFlag: Field nullable flag;
- PrimaryKeyColumnProgramType: Field corresponding program data type;
- PrimaryKeyColumnHungarianPrefix: Hungarian notation prefix;
- PrimaryKeyColumnComma: Comma;
- PrimaryKeyColumnIndex: Field index subscript value;
- PrimaryKeyColumnAutoIncrement: Fields set to auto-increment have a value of "auto_increment", other fields have an empty string;

(4) Index Iteration (including UNIQUE and INDEX type indexes)

- IndexType: Index type, unique index has a value of "unique", non-unique index has a value of "index";
- IndexTypeWithKey: Index type with a key field, unique index has a value of "unique key", non-unique index has a value of "index";
- IndexName: Index name;
- IndexColumns: A comma-separated list of index field names;
- IndexColumnsWithBackQuota: A comma-separated list of index field names, each name enclosed in backticks;

(5) Unique Index Iteration

- UniqueIndexName: Index name;
- UniqueIndexColumns: A comma-separated list of index field names;
- UniqueIndexColumnsWithBackQuota: A comma-separated list of index field names, each name enclosed in backticks;

(6) Non-unique index iteration

- NonUniqueIndexName: Index name;
- NonUniqueIndexColumns: A comma-separated list of index field names;
- NonUniqueIndexColumnsWithBackQuota: A comma-separated list of index field names, each name enclosed in backticks;

(7) Foreign key iteration

- ForeignKeyName: Foreign key name;
- ForeignKeyColumnName: Foreign key field;
- ForeignKeyReferenceTableName: Foreign key reference table name;
- ForeignKeyReferenceColumnName: Foreign key reference field name;

(8) Auto-increment field iteration

- AutoIncColumnName: Field name;
- AutoIncColumnDescription: Field description;
- AutoIncColumnDbType: Field database type;
- AutoIncColumnDbDefaultString: Field default string;
- AutoIncColumnDbDefaultValue: Database default value;
- AutoIncColumnDbNullableFlag: Field nullable flag;
- AutoIncColumnProgramType: Field's corresponding program data type;
- AutoIncColumnHungarianPrefix: Hungarian notation prefix;

(9) Default value iteration

- DefaultValue: Default value string.

# Example

The tool's initial package includes a default `HRSystem` containing two tables: `tdept` and `temployee`. The default example demonstrates how to use this tool to generate initialization scripts and table structure upgrade scripts for C++ structs, Java classes, MyBatis Mappers, MySQL, and Oracle.

In the MySQL and Oracle initialization scripts, a stored procedure named `upgrade_table` is predefined. This stored procedure reads the table creation statement passed in from outside, creates a temporary table, and compares the fields and indexes in the current table for differences. If there are differences in the fields, the data from the current table is imported into the temporary table, and the temporary table replaces the current table after the data import is complete. If there are differences in the indexes, the indexes are recreated; if there are no differences, no operation is performed. In the table structure upgrade script, the tool generates the SQL statement to call the stored procedure based on the table structure data. It is thanks to this tool that the final table structure, after being maintained within the tool, can be guaranteed to be consistent with the tool's internal definition when the upgrade script is executed.
