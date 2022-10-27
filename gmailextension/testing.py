# import base64

# message = "Python is fun"
# message_bytes = message.encode('ascii')
# base64_bytes = base64.b64encode(message_bytes)
# base64_message = base64_bytes.decode('ascii')

# print(base64_message)
# import json

# response = '{"id":"246956be-16b0-457e-ace3-9524e8ad7914","key":"836AD4CC8066700CE90019B1267870D5"}'
# response_object = json.loads(response)
# print(response_object["id"])

import datetime

from datetime import timedelta
now = datetime.datetime.now() + timedelta(days=1)
print("Current date and time: ")
print(type(now))
print(str(now))
    # "2022-10-27T23:12:38.968Z"
string_ver = now.isoformat()
print(type(string_ver))
print(str(string_ver))