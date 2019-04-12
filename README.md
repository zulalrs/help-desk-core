# Help Desk
Arıza kaydı oluşturma, teknisyen atama ve takibini yapma, müşteri memnuniyet anketi oluşturma ve raporlama yapma amacıyla oluşturulmuş bir ASP.NET Core projesidir.

### Öngereklilikler

> -   Visual Studio 2015
> -   Sql Server 2014 Local Db
> -   .Net Framework ^4.5

----------

### []()Repository'yi indirdikten sonra

> **1)**  Solution'ı sağ tıklayıp  **Restore Nuget Packages**'i tıklayınız



> **2)**  _Nuget Package Manager Console_'dan Default Project'i HelpDesk.DAL yaptıktan sonra "**update-database**" komutunu çalıştırınız.
> 
> > **2-a)**  Hata vermesi durumunda "**Rebuild Solution**" yapıp projeyi kapatıp tekrar açabilirsiniz.

## Kullanılan Teknolojiler

-   NTier Project Pattern
-   Repository Entity Pattern
-   ASP.Net Core
-   AutoMapper
-   EntityFramework Core Code First
-   ASP.Net Identity Core
-   Dependency Injection

## Kurulacak Paketler
-   System.Device.Location.Portable(Web)
-   Newtonsoft.Json(Web)
-   Microsoft.AspNetCore.Identity.EntityFrameworkCore(DAL)
-   Microsoft.EntityFrameworkCore.SqlServer(DAL)
-   Microsoft.Extension.Identity.Stores(DAL)
-   Microsoft.EntityFrameworkCore(BLL,DAL,Model,Web)
-   Bootstrap(Web)

----------
