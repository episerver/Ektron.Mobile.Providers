##Ektron.Mobile.Providers

This solution contains providers to allow the Ektron CMS to consume device detection capabilities. It contains:
 * A provider for the 51Degrees(TM) library by 51Degrees.mobi.

##DEPENDENCIES:
This package relies on the public 51Degrees(TM) package from Nuget, as well as Ektron CMS version 9.0 or later.

##BUILD:
Building this solution requires that you have access to several DLLs for your installed version. By default, the hintpath in this project is set to look in your `Program Files (x86)` installation for the Ektron DLLs. If you are building on a machine that does not have Ektron installed, you will need to copy the DLLs from an installed instance, and update the references to point to the correct location.

Open the `Mobile.Providers.51Degrees.sln` in the folder that matches the version you will be deploying to. The project will output a DLL that will allow you to use the 51Degrees(TM) library in your website.

Start by restoring the nuget dependencies, then build the solution.


##NOTES:
Please note that the package dependencies may have different licenses or license requirements. Please consult with the specific provider to learn of their license and other requirements. 

It is understood as of the time of this writing that 51Degrees(TM) uses a freemium model, with an entry level package released under the Mozilla Public License, and a more detailed library available under commercial license. See: https://51degrees.com/compare-data-options. 

Some of the capabilities that the Ektron Device detection relies on are unavailable in the free version of the 51Degrees(TM) library. When creating device groupings, you have three options for the group type:
   - Grouping by breakpoint (resolution)
     This method functions with all versions of the library
   - Grouping by operating system
     This method functions with all versions of the library
   - Grouping by device model
     This method functions with the Premium and Enterprise versions of the library. 

##INSTALL:

###Prerequisite step:
Follow the **BUILD** instructions above to build the project against your currently installed version of Ektron.

###Step 1
Copy the following files from the `Mobile.Providers.51Degrees\bin\Debug` or `Mobile.Providers.51Degrees\bin\Release` folder to the bin folder in your Ektron website:
  * Mobile.Providers.FiftyOneDegrees.dll
  * FiftyOne.Foundation.dll

###Step 2
Get the latest version from the 51Degrees website or copy the following files from the Mobile.Providers.51Degrees\App_Data folder to the App_Data folder in your website, or 
  * 51Degrees.dat

###Step 2a
On version 8.6 and 8.7, copy the workarea folder to the website, preserving the folder structure so that the files overwrite the existing files.

###Step 3
### Version 9.0 and 9.1:
Modify the `ektron.cms.framework.unity.config` file in the root of your website. Find the following line:
```xml
<typeAlias alias="BusinessObjects.IDeviceInfoProvider" type="Ektron.Cms.Mobile.WURFLProvider, Ektron.Cms.Mobile"/>
```
and change the type mapping to match the following:
```xml
<typeAlias alias="BusinessObjects.IDeviceInfoProvider" type="Mobile.Providers.FiftyOneDegrees.FiftyOneDegreesProvider, Mobile.Providers.FiftyOneDegrees"/>
```
### Version 8.6 and 8.7:
Modify the `ektron.cms.framework.unity.config` file in the root of your website. Find the following line:
```xml
<typeAlias alias="BusinessObjects.IWURFLProvider" type="Ektron.Cms.Mobile.WURFLProvider, Ektron.Cms.Mobile"/>
```
and change the type mapping to match the following:
```xml
<typeAlias alias="BusinessObjects.IWURFLProvider" type="Mobile.Providers.FiftyOneDegrees.FiftyOneDegreesProvider, Mobile.Providers.FiftyOneDegrees"/>
```


###Step 4
Modify the web.config in your website, and find the configuration/appSettings section. Add the following key to it
```xml
<add key="51DegreesLocation" value="{location of dat file}"/>
```
Replace the {location of dat file} with the location and name of your dat file, for example:
```xml
<add key="51DegreesLocation" value="~/App_Data/51Degrees.dat"/>
```
###Step 5
In the Web.config `appSettings` section, make sure that: `ek_EnableDeviceDetection` is set to "true":
```xml
<add key="ek_EnableDeviceDetection" value="true"/>
```
