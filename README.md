![BankSync Logo](https://raw.githubusercontent.com/bartosz-jarmuz/BankSync/master/logotype.png)

Simple **personal-use application** which exports data from bank accounts to a common format and allows to import it to FinTech solutions - without exposing your account to 3rd parties.

## Purpose
The app was written because I needed a simple solution to export my financial data from a couple of banks that I use and **'flatten' it to a simple intermediate format.**
The standardized data can then be easily imported to 3rd party FinTech solutions, such as Wallet by BudgetBakers or AndroMoney. (Although I ended up using PowerBi:) ).

## Features
### Export data from:

#### Polish banks:
 - PKO 
 - Citibank
 
#### Online Shopping
 - Allegro.pl

### Enrich data:
 - Exporting purchase history from Allegro **correlates this data with payment information**, so that generic entries such as PayU/PayPal identifiers are enriched with 'what I bought'.
 - Categorize the data into categories and subcategories you define (in XML), based on the payment info, recipient etc.
 
### Output formats:
 - JSON
 - XLSX
 - GoogleSheets direct import
 
### Security
 - Your crendentials (if you want to save them) are only stored locally on your machine and always encrypted with [DPAPI](https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection)

## PSD2
This app is not based on the OpenBanking API and does not leverage the benefits of PSD2. 
The reason is that the application is for personal use only (i.e. not to be sold or even distributed to other users for free) and it is only works by 'pretending to be you using the web browser without UI'. 
Registering an application for the use of those APIs in my bank was a bit overcomplicated and an overkill in the long run.
It is using the .net HttpClient to send HTTP Requests - the ones you can check by using Developer Tools in you web browser.

## Final note
The code is there if you want to compile and use it - the application is only partially generic, which means you might need to adjust it to yourself - especially if PKO is not your bank.
Also, when PKO website changes, the web requests will likely have to be reworked. The price of not using the Open Banking APIs.
 
 




 
