from flask import Flask, request, Response
from threading import Thread
import slack_sdk as slack
from slack_sdk.errors import SlackApiError
from slack_sdk.signature import SignatureVerifier
import os
from pathlib import Path
from dotenv import load_dotenv
import requests
from datetime import datetime as dt
from datetime import timedelta
import base64
import json
from modal_def import *

SERVER_URL = "https://youshallnotpassbackend.azurewebsites.net"
VAULT_ENDPOINT = "vault"

FRONTEND_URL = "http://youshallnotpass.org"
FRONTEND_PASSWORD_VIEW = "view"

# get bot token from env variable
env_path = Path("..") / ".env"
load_dotenv(dotenv_path=env_path)
bot_api_token = os.environ["BOT_TOKEN"]
signature_verifier = SignatureVerifier(os.environ["SLACK_SIGNING_SECRET"])

client = slack.WebClient(token=bot_api_token)

app = Flask("")


@app.route("/")
def home():
  return "You Shall Not Pass bot is alive!"


@app.route("/youshallnotpass-shortcut", methods=["POST"])
def shortcut():
  if not signature_verifier.is_valid_request(request.get_data(),
                                             request.headers):
    return Response("Invalid request"), 403

  if "payload" in request.form:
    payload = json.loads(request.form["payload"])

    if payload["type"] == "shortcut" and payload[
        "callback_id"] == "youshallnotpass_shortcut":
      # Open a new modal by a global shortcut
      try:
        client.views_open(trigger_id=payload["trigger_id"], view=global_modal)
        return Response(), 200

      except SlackApiError as e:
        code = e.response["error"]
        print("Error code: " + code)
        return Response(f"Failed to open a modal due to {code}"), 200

    if payload["type"] == "view_submission" and payload["view"][
        "callback_id"] == "youshallnotpass-modal-id":
      # Handle a data submission request from the modal
      submitted_data = payload["view"]["state"]["values"]
      user_id = payload["user"]["id"]
      channel_id = submitted_data["conversation-block-id"]["conversation-id"][
        "selected_conversation"]

      max_accesses = None if not "value" in submitted_data["access-block-id"][
        "access-id"] else int(
          submitted_data["access-block-id"]["access-id"]["value"])

      expiration_hrs = None if None == submitted_data["expiration-block-id"][
        "expiration-id"]["selected_option"] else int(
          submitted_data["expiration-block-id"]["expiration-id"]
          ["selected_option"]["value"])

      error = upload_and_post_secret(
        submitted_data["secret-block-id"]["secret-id"]["value"],
        submitted_data["label-block-id"]["label-id"]["value"], expiration_hrs,
        max_accesses, channel_id, user_id)

      return Response(error), 200

  return Response(), 404


# Endpoint for Slack slash command
@app.route("/youshallnotpass-slashcmd", methods=["POST"])
def password_command():
  data = request.form
  channel_id = data.get("channel_id")

  if (data.get("text") == ""):
    return Response(
      "Error: can't send an empty secret. Try '/youshallnotpass [your secret]'"
    ), 200

  error = upload_and_post_secret(data.get("text"), None, None, None,
                                 channel_id, data.get("user_id"))

  return Response(error), 200


def upload_and_post_secret(secret, label, expiration_hrs, max_accesses,
                           channel_id, user_id):
  if secret == None or secret == "":
    return "Error: can't send an empty secret", None

  if label == None:
    label = "Password sent via Slack"

  if expiration_hrs == None:
    expiration_hrs = 24

  if max_accesses == None:
    max_accesses = 1

  secret_data = {
    "contentType": 2,
    "label": label,
    "expirationDate":
    (dt.utcnow() + timedelta(hours=expiration_hrs)).isoformat(),
    "maxAccessCount": max_accesses,
    "data": base64.b64encode(secret.encode("ascii")).decode("ascii")
  }

  response = requests.post(url=f"{SERVER_URL}/{VAULT_ENDPOINT}",
                           json=secret_data)

  if response.ok:
    response_data = json.loads(response.text)
    secret_id = response_data["id"]
    secret_key = response_data["key"]

    link = f"{FRONTEND_URL}/{FRONTEND_PASSWORD_VIEW}?id={secret_id}&key={secret_key}"

    client.chat_postMessage(
      channel=channel_id,
      text=(f"<@{user_id}> has sent a secret to the chat: {link}"))
    return

  return "An error was encountered while uploading your secret: please try again"


def run():
  app.run(host="0.0.0.0", port=8080)


def keep_alive():
  t = Thread(target=run)
  t.start()
