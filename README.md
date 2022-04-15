# SeleniumCSharp
Sample Selenium framework using C# , .Net core 3.1 and Selenium 4
## Intall the project
- Clone the project
- Open the solution
- Build solution
- From the menue Test --->Configure run settings --->Select solution wide runsettings file ---> From the project path---> Runsettings ---> Default.runsettings
- From the solution explorer open the folder Tests ---> RealWorldApp 
- Open the test file RealWorldApp.cs
- Run the test
- The will run in chrome wb driver, using test domain https://www.demoblaze.com/
## Framework structure
Ther is a seperate class for each page. A page is a sub-page(Such as contact modal) or a completely unique page stands alone (Such as the login page) 

### Elements
Elements are needed to link the test to the correct ID, class or Xpath on the web enviroment.

### Naming conventions
Elements should have friendly names in the code to easily discover what the Elements should do. The friendly name of the element corresponds to the name on the screen + type, for example: **ContactModalSendMessageButton** or **ContactModalMessageField**

### Actions
An Action contains methods with actions and are excuted multible times as well as definitions of elements that contains a unic ID
Example:
```
public IwebElement GetOptionFromList(int optionNumber)
{
   return _driver.FindElementSafe(By.Id("option" + optionNumber));
}
```
### How to add a new page to the project
Create a new folder in the Pages folder. Place an Elements and Actions file with aonstructor in it 

Open the WebAppInitializer file in the Engn folder and add two new lines:
```
public <PageName>Actions<PageName>Actions => Resolve<PageName>Actions();
public <PageName>Elements<PageName>Elements => Resolve<PageName>Elements();
```
In this way will ensure that your classes are registers and you can call them from the test page

### Example RealWorldApp
The framework comes with a sample example RealWorldApp test that will clarify the framework structure
