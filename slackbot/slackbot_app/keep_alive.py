from flask import Flask, request, Response
from threading import Thread
import slack_sdk as slack
import os
from pathlib import Path
from dotenv import load_dotenv
import requests
from datetime import datetime as dt
from datetime import timedelta
import base64
import json

SERVER_URL = 'https://youshallnotpassbackend.azurewebsites.net'
VAULT_ENDPOINT = '/vault'

FRONTEND_URL = 'http://youshallnotpass.org'
FRONTEND_PASSWORD_VIEW = '/view'

# get bot token from env variable
env_path = Path('..') / '.env'
load_dotenv(dotenv_path=env_path)
bot_api_token = os.environ['bot_token']

client = slack.WebClient(token=bot_api_token)

app = Flask('')


@app.route('/')
def home():
  return "You Shall Not Pass bot is alive!"


# Endpoint for Slack slash command
# Default settings: one-day expiration, one-time access
# Supports text only
@app.route('/youshallnotpass', methods=['POST'])
def password_command():
  data = request.form
  channel_id = data.get('channel_id')

  if (data.get('text') == ''):
    return Response(
      "Error: can't send an empty secret. Try '/youshallnotpass [your secret]'"), 200

  # send request to our backend for a link
  secret_data = {
    "contentType": 2,
    "label": "Password sent via Slack",
    "expirationDate": ((dt.utcnow() + timedelta(days=1)).isoformat()),
    "maxAccessCount": 1,
    "data": base64.b64encode(data.get('text').encode('ascii')).decode('ascii')
  }

  response = requests.post(url=SERVER_URL + VAULT_ENDPOINT, json=secret_data)

  if response.ok:
    response_data = json.loads(response.text)

    link = FRONTEND_URL + FRONTEND_PASSWORD_VIEW + '?id=' + response_data[
      'id'] + '&key=' + response_data['key']

    client.chat_postMessage(channel=channel_id, text=("<@" + data.get('user_id') + "> has sent a secret to the chat: " + link))
    return Response(), 200

  else:
    return Response(), 500


def run():
  app.run(host='0.0.0.0', port=8080)


def keep_alive():
  t = Thread(target=run)
  t.start()
