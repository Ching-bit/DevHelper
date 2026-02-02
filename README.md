# 介紹
一個通用的客戶端模板，主要目的是將客戶端開發的一些通用的功能實現，並實現組件可插拔式的配置化，能快速地修改為實際項目所需要的形式。項目主要使用了以下開源框架：

**Avalonia**：基本框架，社區開源，Avalonia可看成是WPF的升級版，在WPF上進行了多項優化，而且最重要地，Avalonia是跨平台的，一套代碼支持Windows、Linux、MacOS；

**Semi.Avalonia**、**Irihi.Ursa**：UI庫，三方開源；

**CommunityToolkit.Mvvm**：MVVM設計模式，Microsoft開源；

**Dock.Avalonia**：界面浮動、拖拉效果，三方開源；

**NLog**：日誌，三方開源。

項目中的類名常以Uni開頭，表示Universal，通用的。

# 開發

## 1. 插件

插件即是一個能被框架啟動、保存的特定類型。Framework.Common中定義了插件接口IPlugin，其中定義了插件的生命週期方法。框架將在客戶端開啟、登錄、退出時調用相應方法。

開發一個新插件：

（1）創建插件接口，該接口繼承IPlugin；

（2）創建接口的實現類。必須是3層繼承關係：IPlugin -> 插件接口 -> 插件實現類。插件接口和實現類需要位於最外層命名空間；

建議：所有項目的類都放在最外層的項目名稱命名空間；

（3）在sys\Plugins.xml中增加該插件的配置，Assembly填寫該插件所在dll的名稱，不加dll擴展名；Name填寫實現類的名稱。

框架啟動時將根據Plugins.xml中的配置利用反射創建插件實例並保存，使用時以Global.Get\<T>()獲取插件，其中T是插件接口類型。

## 2. MVVM設計模式

Avalonia、WPF本身支持面向事件的開發，但更推薦使用MVVM設計模式。

MVVM即是指——M：Model，代表數據；V：View，代表界面；VM：ViewModel，代表邏輯。通過綁定（Binding）將界面元素於ViewModel中的數據關聯，使得客戶端的開發成為面向數據的開發。

在本項目中，MVVM的基類都定義在Framework.Common中。

**View**：窗口繼承UniWindow類，用戶控件（指不能獨立展現，需作為窗口的一部分）繼承UniPanel類；

**ViewModel**：繼承UniViewModel類。

框架將根據類名自動將View和ViewModel關聯，關聯規則：對於名稱為XxxView或Xxx（不以View結尾）的View類，對應的ViewModel類名為XxxViewModel。View和ViewModel或者位於相同命名空間，或者分別位於Xxx.Views、Xxx.ViewModels命名空間。因此再次建議——

建議：所有項目的類都放在最外層的項目名稱命名空間。

**Model**：繼承UniModel類。UniModel類目前僅繼承了Communitytoolkit.Mvvm庫的ObservableObject類，未做其他處理。Model中的屬性，可通過Communitytoolkit.Mvvm庫提供的在私有變量上添加[ObservableProperty]特性來快捷地創建（否則就需要繼承INotifiedPropertyChanged接口來實現，多數情況下不需要如此勞煩）。

## 3. 控件

**Control.Basic** 項目用於提供封裝好的可複用的控件。

目前主要有一個對話框控件，創建一個對話框——

（1）創建對話框用戶控件，繼承Avalonia的UserControl，不要繼承UniPanel，**Irihi.Ursa** 庫會在顯示對話框的方法中將View和ViewModel關聯；

（2）用戶控件中添加ConfirmDialog控件，這是一個TemplatedControl，設置其ReturnParameter參數，用於對話框確認後返回；

（3）用戶控件的ViewModel繼承ConfirmDialogViewModel；

（4）可使用 await Dialog.ShowCustomModal<View, ViewModel, ReturnClass>() 方法展示對話框，對話框被確認或取消後將返回 ConfirmDialogResult 參數，其中包含對話框點擊的結果和返回的參數。

## 4. 菜單

菜單的View繼承UniMenu類。UniMenu本身是UniPanel，但增加了菜單生命週期的方法，這些方法將在菜單打開、顯示、隱藏、關閉時相應被調用。菜單的ViewModel與上述相同，繼承UniViewModel並按照名稱與View對應。

菜單開發好後，在sys\Menus.xml中配置，框架將按照配置文件的層級關係在登錄後的左側菜單樹型結構中進行展示。

## 5. 多語言

多語言按照Avalonia本身的動態關聯資源名稱的方式實現，對於一些無法動態關聯的字段，目前框架也做了處理。

目前只需在UniClient項目的Resources\Lang中配置各個語言的axaml資源文件。有一個簡單的Python腳本，更簡化了開發。只需配置LangResources.csv文件，運行GenLangResources.py，即會生成相應axaml資源文件。

添加新語言：（1）在Framework.Common中的Helpers\LanguageConst.cs添加新的語言的常量，其中常量名稱就是在設置中語言列表展示的選項，值用來關聯資源文件；（2）按上述生成新語言的axaml資源文件。即可完成新語言的支持。

## 6. Roslyn 編譯期代碼生成

.Net平台提供了Roslyn編譯接口，可利用其提供的機制和C#的特性（類似Java的註解），進一步簡化代碼。

目前開發了 **Attributes.Avalonia** 項目，可利用一條特性方便地為控件添加StyledProperty、DirectProperty，以及為幫助類添加AttachedProperty。項目已打包上傳NuGet，可具體查看項目的說明文件。

## 7. 客戶端升級

支持了以 Http 部署升級服務。

升級地址配置在 sys\AppConf.xml 的 UpdateAddrs 配置項中，多個地址以英文分號隔開。升級時將首先在此URL地址下查找 UpdateConf.xml 文件，UpdateConf.xml 文件示例如下。

```
<UpdateConf>
  <UpdateListPath>/${Platform}/FileList.xml</UpdateListPath>
  <ChangeLogPath>/changelogs/ChangeLog.txt</ChangeLogPath>
  <RemoteClientDir>/${Platform}/client</RemoteClientDir>
  <IsForce>false</IsForce>
</UpdateConf>
```

UpdateListPath: 客戶端文件列表路徑，其中記錄升級服務器上客戶端文件列表及其MD5值，客戶端登錄後將逐一比對此文件中記錄的文件MD5值和本地文件MD5值，若有不一致即判斷需要升級；

ChangeLogPath：升級內容文本文件路徑，支持多語言，若該路徑有值，客戶端將彈出升級內容提示對話框並展示改文本內容，否則將只彈出需要升級的提示；

RemoteClientDir：升級服務器客戶端文件所在路徑，支持多平台配置；

IsForce：是否強制，若不強制，用戶可取消升級並繼續登錄。

升級服務器一個可用的文件部署目錄——
```
webapps
  - uniserver
    - UpdateConf.xml // 升級配置文件
    - changelogs     // 升級內容文本
      - ChangeLog.en.txt  // 英文升級內容
      - ChangeLog.zht.txt // 中文升級內容
    - windows_x64    // Windows x64 客戶端目錄
      - FileList.xml // 客戶端文件信息文件
      - client       // 客戶端文件目錄
        - ...        // 客戶端文件
    - linux_x64      // Linux x64 客戶端目錄
      - FileList.xml // 客戶端文件信息文件
      - client       // 客戶端文件目錄
        - ...        // 客戶端文件
    - WEB-INF
      - web.xml      // 配置文件，可進行一些Http安全配置
```

## 8. 前後台通信

### 8.1 RPC vs MQ

當系統需要即時響應並依賴結果時，用RPC；當系統需要解耦、異步處理或削峰填谷時，用MQ。

### 8.2 RPC

目前常用Thrift和gPRC。

#### 8.2.1 Thrift

Plugin.Thrift插件實現了Thrift的RPC調用。一些實踐建議——

（1）Thrift接口定義文件示例

```
namespace netstd Plugin.Thrift

struct UserServiceLoginRequest {
    1: string username
    2: string password
}

struct UserServiceLoginResponse {
    1: i32 errorCode
    2: string errorMsg
}

service UserService {
    UserServiceLoginResponse Login(1: UserServiceLoginRequest request)
}
```

可按service分文件定義，每個thrift文件定義一個service，命名空間都為項目名的最外層命名空間。若有struct定義封裝請求、應答，以 **service名稱+方法名稱+Request/Response** 命名，變量名以小駝峰命名。Thrift支持方法定義多入參，但建議將方法的入參都封裝成一個struct，使得每個方法都只有一個struct入參，與GRPC有一致習慣，對於協議更換更有利（GRPC要求每個方法只能有一個入參）。

**注意**：thrift.exe（0.22.0版本）生成的C#代碼中，上方會加一段針對NETSTANDARD2_0_OR_GREATER宏的判斷，但.Net後續版本沒有這個宏，會編譯報錯。這段編譯判斷代碼需要被刪去。

（2）Plugin.Thrift

對於Thrift，每個service都將生成一個client，目前將各client都按照同樣的服務端地址（即登錄時選擇的地址）鏈接。在插件登錄前的OnLogging()方法中進行初始化。

thrift.exe 生成的代碼都放在 Plugin.Thrift 項目的 AutoGen\Services 下，初始化放在 AutoGen\ThriftService.cs 中。每定義一個新的service，都要在 AutoGen\ThriftService.cs 中進行添加。這個文件連同 thrift.exe 生成的代碼，都可由一個開發輔助工具來自動生成（這個工具可後續開發）。

（3）服務調用示例
```
await Global.Get<IThriftService>().DoServiceAsync<UserService.Client>(async client =>
{
    
});
```

將在服務調用前後進行在客戶端池中獲取Thrift的Client、調用後歸還等操作。

#### 8.2.2 GRPC

Plugin.Grpc插件實現了GRPC調用。一些實踐建議——

（1）gRPC接口定義文件示例

```
syntax = "proto3";

package services;
option csharp_namespace = "Plugin.Grpc";

message UserServiceLoginRequest {
    string username = 1;
    string password = 2;
}

message UserServiceLoginResponse {
    int32 errorCode = 1;
    string errorMsg = 2;
}

service UserService {
    rpc Login(UserServiceLoginRequest) returns (UserServiceLoginResponse) {}
}
```

gRPC要求每個RPC只能有一個入參，故若有多個參數必須封裝為一個message。命名方式可參考8.2.1節Thrift。

（2）Plugin.Grpc

對於Grpc，每個service都包含一個client，目前各client都共用一個GrpcChannel（即登錄時選擇的地址）。在插件登錄前的OnLogging()方法中進行初始化。

protoc.exe 生成的代碼都放在 Plugin.Grpc 項目的 AutoGen\Services 下，初始化放在 AutoGen\GrpcService.cs 中。每定義一個新的service，都要在 AutoGen\GrpcService.cs 中進行添加。這個文件連同 protoc.exe 生成的代碼，都可由一個開發輔助工具來自動生成（這個工具可後續開發）。

（3）服務調用示例
```
UserServiceLoginResponse response = Global.Get<IGrpcService>().GetService<UserService.UserServiceClient>().Login(request);
```
