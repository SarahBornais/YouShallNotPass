import base64
from email.message import EmailMessage
import base64
import json
import flask

from datetime import timedelta
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from urllib.parse import urlparse 
from urllib.parse import parse_qs

app = flask.Flask(__name__)
app.secret_key = "TODO: authenticate"

# If modifying these scopes, delete the file token.json.
SCOPES = ["https://mail.google.com/"]
url = "https://youshallnotpassbackend.azurewebsites.net/vault"
API_SERVICE_NAME = "gmail"
API_VERSION = "v1"
CLIENT_SECRETS_FILE = "credentials.json"
GMAIL_DRAFTS_URL = "https://mail.google.com/mail/u/0/#drafts"

def create_message(secret_url, secret_key):
    message = EmailMessage()
   
    shallnotpass_data = f"The sender has sent you encrypted sensitive data. Click on the link: https://youshallnotpass.org/view?id={secret_url} and input the key: {secret_key} to access the secret."

    message.set_content(shallnotpass_data)

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

    print(flask.request.url)
    create_draft(client, flask.request.url)
    # return flask.render_template('index.html')    
    return flask.redirect(GMAIL_DRAFTS_URL)
    
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

def create_draft(service, url):  
    try:
        # Call the Gmail API
        parsed_url = urlparse(url)
        id = parse_qs(parsed_url.query)['id'][0]
        key = parse_qs(parsed_url.query)['key'][0]

        new_message = create_message(id, key)
        print(new_message)
        
        new_draft = service.users().drafts().create(
            userId='me', 
            body = new_message).execute()
            
    except HttpError as error:
        # TODO(developer) - Handle errors from gmail API.
        print(f'An error occurred: {error}')
    
    # return flask.render_template('index.html')    
    return flask.redirect(GMAIL_DRAFTS_URL)

if __name__ == "__main__":
    app.run()
    # app.run(ssl_context='adhoc')