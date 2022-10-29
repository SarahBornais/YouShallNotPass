import os.path
import base64
from email.message import EmailMessage
import re
import base64
import requests
import json
import datetime

from datetime import timedelta
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from flask import Flask, render_template

app = Flask(__name__)

# If modifying these scopes, delete the file token.json.
SCOPES = ['https://mail.google.com/']
url = 'https://youshallnotpassbackend.azurewebsites.net/vault'

def apply_shallnotpass(s):
    print(s)
    secret_start = s.find("/shallnotpass")
    secret_end = s.find("/endshallnotpass") 
    if (secret_start!=-1 and secret_end!=-1):
        protected_secret = protect_secrets(s[secret_start + 13:secret_end])
        return s[:secret_start] + protected_secret + apply_shallnotpass(s[secret_end + 16:])
    else:
        return ""

def protect_secrets(s):
    s.strip()

    message_bytes = s.encode('ascii')
    base64_bytes = base64.b64encode(message_bytes)  
    base64_message = base64_bytes.decode('ascii')
    expiry_date = datetime.datetime.now() + timedelta(days=1) #defaults to one day expiry
    expiry_str = expiry_date.isoformat()
    print(expiry_str)

    obj = {
    "contentType": 0,
    "label": "string",
    "expirationDate": expiry_str,
    # "2022-10-27T23:12:38.968Z"
    "maxAccessCount": 3,
    "timesAccessed": 0,
    "data": base64_message
    }
    
    response = requests.post(url, json = obj)
    response_object = json.loads(response.text)
    id = response_object["id"]
    key = response_object["key"]
    get_secret_url = f"https://youshallnotpass.org/view?id={id}&key={key}"
    print(get_secret_url)

    return get_secret_url

def create_message(original_draft):
    message = EmailMessage()

    data = ""

    for p in original_draft["message"]["payload"]["parts"]:
        if p["mimeType"] in ["text/plain"]:
            data += base64.urlsafe_b64decode(p["body"]["data"]).decode("utf-8")
            # print(data)

    shallnotpass_data = apply_shallnotpass(data)

    message.set_content(shallnotpass_data)

    for metadata in original_draft["message"]["payload"]["headers"]:
        if (metadata["name"] == "To"):
            message['To'] = metadata["value"]
        if (metadata["name"] == "Subject"):
            message['Subject'] = metadata["value"]
    
    message['From'] = 'me'
    
    encoded_message = base64.urlsafe_b64encode(message.as_bytes()).decode()
    
    new_message = {
        'message': {
            'raw': encoded_message
        }
    }

    return new_message
    
@app.route('/')
def homepage():
    return render_template('index.html')
    

#Using the below, the popup message appears when the button is clicked on the webpage.
#0x00001000 - This makes the popup appear over the browser window
@app.route('/sendmail')
def sendmail():  
    print("sendmail was triggered")
    """Shows basic usage of the Gmail API.
    Lists the user's Gmail drafts.
    """
    creds = None
    # The file token.json stores the user's access and refresh tokens, and is
    # created automatically when the authorization flow completes for the first
    # time.
    if os.path.exists('token.json'):
        creds = Credentials.from_authorized_user_file('token.json', SCOPES)
    # If there are no (valid) credentials available, let the user log in.
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(
                'credentials.json', SCOPES)
            creds = flow.run_local_server()
        # Save the credentials for the next run
        with open('token.json', 'w') as token:
            token.write(creds.to_json())

    try:
        # Call the Gmail API
        service = build('gmail', 'v1', credentials=creds)
        # results = service.users().drafts().list(userId='me').execute()
        results = service.users().drafts().list(userId='me').execute()
        drafts = results.get('drafts', [])

        if not drafts:
            print('No drafts found.')
            return
        print('drafts:')

        # drafts[0] is the most recently created draft.
        draft = drafts[0]
        original_draft = service.users().drafts().get(userId='me', id=draft["id"], format="full").execute()

        new_message = create_message(original_draft)
        print(new_message)
        
        new_draft = service.users().drafts().update(
            userId='me', 
            id = original_draft["id"],
            body = new_message).execute()

        
        service.users().drafts().send(
            userId='me',
            body = new_draft).execute()

    except HttpError as error:
        # TODO(developer) - Handle errors from gmail API.
        print(f'An error occurred: {error}')
    
    return render_template('index.html')

if __name__ == "__main__":
    app.run()