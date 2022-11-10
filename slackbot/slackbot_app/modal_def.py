global_modal = {
  "type":
  "modal",
  "callback_id":
  "youshallnotpass-modal-id",
  "title": {
    "type": "plain_text",
    "text": "You Shall Not Pass!"
  },
  "submit": {
    "type": "plain_text",
    "text": "Submit"
  },
  "close": {
    "type": "plain_text",
    "text": "Cancel"
  },
  "blocks": [{
    "type": "input",
    "block_id": "secret-block-id",
    "label": {
      "type": "plain_text",
      "text": "Secret",
    },
    "element": {
      "action_id": "secret-id",
      "type": "plain_text_input",
    }
  }, {
    "type": "input",
    "block_id": "label-block-id",
    "label": {
      "type": "plain_text",
      "text": "Description of Secret",
    },
    "element": {
      "action_id": "label-id",
      "type": "plain_text_input",
    },
    "optional": True,
  }, {
    "type": "input",
    "block_id": "access-block-id",
    "label": {
      "type": "plain_text",
      "text": "Maximum Number of Accesses",
    },
    "element": {
      "action_id": "access-id",
      "type": "number_input",
      "is_decimal_allowed": False,
      "min_value": "1",
    },
    "optional": True,
  }, {
    "type": "input",
    "block_id": "expiration-block-id",
    "label": {
      "type": "plain_text",
      "text": "Expiration Time",
    },
    "element": {
      "action_id":
      "expiration-id",
      "type":
      "static_select",
      "options": [
        {
          "text": {
            "type": "plain_text",
            "text": "1 hour"
          },
          "value": "1"
        },
        {
          "text": {
            "type": "plain_text",
            "text": "24 hours"
          },
          "value": "24"
        },
        {
          "text": {
            "type": "plain_text",
            "text": "48 hours"
          },
          "value": "48"
        },
        {
          "text": {
            "type": "plain_text",
            "text": "1 week"
          },
          "value": "168"
        },
        {
          "text": {
            "type": "plain_text",
            "text": "1 month"
          },
          "value": "720"
        },
      ]
    },
    "optional": True,
  }, {
    "type": "input",
    "block_id": "conversation-block-id",
    "label": {
      "type": "plain_text",
      "text": "Send Secret To:",
    },
    "element": {
      "action_id": "conversation-id",
      "type": "conversations_select",
    }
  }]
}
