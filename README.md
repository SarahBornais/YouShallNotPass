# YouShallNotPass

A secret sharing platform with expiring links. Up and running here: http://youshallnotpass.org

[![Netlify Status](https://api.netlify.com/api/v1/badges/b06b9705-096f-49e4-adc8-78b6b75e7692/deploy-status)](https://app.netlify.com/sites/youshallnotpassfrontend/deploys)

## Backend
Backend is running on Azure at https://youshallnotpassbackend.azurewebsites.net

To run locally,
1. First, install [.Net 6](https://dotnet.microsoft.com/en-us/download)
2. Define a server key
    1. Navigate to `Backend/Api/`
    2. Run `dotnet user-secrets init`
    3. Go to this [site](https://www.allkeysgenerator.com/Random/Security-Encryption-Key-Generator.aspx) and generate a 128 bit key with hex format (for example, `217A25432A462D4A614E645266556A58`)
    4. Run `dotnet user-secrets set "ServerKey" "[key]"`
3. Two options:
    1. Visual Studio
        1. Install [Visual Studio](https://visualstudio.microsoft.com/vs/community/) (make sure to inlcude the web development workload)
        2. Open `Backend/YouShallNotPassBackend.sln` in Visual Studio
        3. Run the `YouShallNotPassBackend` application
    2. Command Line
        1. Navigate to `Backend/Api/`
        2. Run `dotnet run`, the url will show in the terminal
        3. The root index is a swagger UI that shows all of the APIs and how to use them (see `/Backend/ContentType.cs` for the enum definitions)

To run the Api Tests,
1. First, install [.Net 6](https://dotnet.microsoft.com/en-us/download)
2. If testing a local deployment, run the server locally
3. Two options:
    1. Visual Studio (only available on Windows)
        1. Install [Visual Studio](https://visualstudio.microsoft.com/vs/community/) (make sure to inlcude the web development workload)
        2. Open `Backend/YouShallNotPassBackend.sln` in Visual Studio 
        3. Go to `Test -> Configure Run Settings -> Select Solution Wide runsettings File` and select one of `RunSettings/[local|azure].runsettings`
        4. Right-click on the `YouShallNotPassBackendApiTests` project in Solution Explorer and click `Run Tests`
    1. Command Line
        1. Navigate to `Backend/ApiTests/`
        2. Run `dotnet test -s RunSettings/[local|azure].runsettings`

## Frontend

To run the frontend locally, first ensure that Node, React, and Typescript are installed. Then, run:

### `npm install`

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.\
You will also see any lint errors in the console.

### `npm run build`

Builds the app for production to the `build` folder.\
It correctly bundles React in production mode and optimizes the build for the best performance.

## Gmail Extension

### Running the Python Server Locally Instructions 
Ensure you have python, pip installed then run the following commands
### `cd YouShallNotPass\gmailextension\sendmail`

Create the virtual environment and activate it

### `python -m venv venv`
### `venv\Scripts\activate`

Then, to restore the packages in the virtual environment

### `pip install -r requirements.txt`

(temporary step for October 29th testing) copy the credentials.json file into the sendmail folder

Run the server 

### `python app.py`

### Add the gmail extension to chrome in developer mode
Sign in to your Google Account on a Chrome device or Chrome browser on a Windows, Mac, or Linux computer.

Go to chrome://extensions/.

At the top right, turn on Developer mode.

Click Load unpacked.

Find and select extension folder: `\YouShallNotPass\gmailextension\chrome\templates`

Open a new tab in Chrome then click the extension. You will see a button that says 'Run Shall Not Pass'

### Sending an email
	1. Construct an email in gmail, as you do normally, do not send the draft.
	2. Protect text secrets starting with /shallnotpass and ending with /endshallnotpass
	3. Use the 'Run Shall Not Pass' button from the chrome extension you added in the previous section.
	4. You may be asked to sign in.

