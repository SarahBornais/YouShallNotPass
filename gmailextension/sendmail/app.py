import os.path
import base64
from email.message import EmailMessage
import re
import base64
import requests
import json
import datetime
import flask

from datetime import timedelta
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError

app = flask.Flask(__name__)
app.secret_key = 'TODO: authenticate'

# If modifying these scopes, delete the file token.json.
SCOPES = ['https://mail.google.com/']
url = 'https://youshallnotpassbackend.azurewebsites.net/vault'
API_SERVICE_NAME = "gmail"
API_VERSION = "v1"
CLIENT_SECRETS_FILE = "credentials.json"

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
    "contentType": 2,
    "label": "string",
    "expirationDate": expiry_str,
    # "2022-10-27T23:12:38.968Z"
    "maxAccessCount": 3,
    "timesAccessed": 0,
    "data": base64_message
    }
    
    response = requests.post(url, json = obj)
    response_object = json.loads(response.text)
    print("ANNA: response object is ")
    print(response_object)
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
    if 'credentials' not in flask.session:
        return flask.redirect('authorize')

    credentials = Credentials(
        **flask.session['credentials'])

    client = build(
        API_SERVICE_NAME, API_VERSION, credentials=credentials)

    create_draft(client)
    return flask.render_template('index.html')
    
@app.route('/authorize')
def authorize():
    # flow instance manages OAuth 2.0 Authorization 
    flow = InstalledAppFlow.from_client_secrets_file(
        CLIENT_SECRETS_FILE, scopes=SCOPES)
    flow.redirect_uri = flask.url_for('oauth2callback', _scheme='https', _external=True)
    authorization_url, state = flow.authorization_url(
        access_type='offline'
    )

    flask.session['state'] = state

    return flask.redirect(authorization_url)

@app.route('/oauth2callback')
def oauth2callback():
    state = flask.session['state']
    flow = InstalledAppFlow.from_client_secrets_file(
        CLIENT_SECRETS_FILE, scopes=SCOPES, state=state)
    flow.redirect_uri = flask.url_for('oauth2callback', _scheme='https', _external=True)

    # use server's response to fetch the OAuth 2.0 tokens.
    auth_response_http = flask.request.url
    if "http:" in auth_response_http:
        auth_response_http = "https:" + auth_response_http[5:]
    authorization_response = auth_response_http
    flow.fetch_token(
        authorization_response=authorization_response
        )

    # Store the credentials and token for the session
    credentials = flow.credentials
    flask.session['credentials'] = {
        'token': credentials.token,
        'refresh_token': credentials.refresh_token,
        'token_uri': credentials.token_uri,
        'client_id': credentials.client_id,
        'client_secret': credentials.client_secret,
        'scopes': credentials.scopes
    }
    return flask.redirect('/')

def create_draft(service):  
    try:
        # Call the Gmail API
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

        
        # service.users().drafts().send(
        #     userId='me',
        #     body = new_draft).execute()

    except HttpError as error:
        # TODO(developer) - Handle errors from gmail API.
        print(f'An error occurred: {error}')
    
    return flask.render_template('index.html')

if __name__ == "__main__":
    app.run()
    # app.run(ssl_context='adhoc')