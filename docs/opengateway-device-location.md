# OpenGateway Device Location (reference example)

This is a reference snippet for the [CAMARA / Open Gateway](https://opengateway.telefonica.com/) **Device Location**
API, which lets an authorized client verify or retrieve the approximate network-reported location of a mobile
device tied to a phone number.

This example is documentation only — it is not wired into the wormwars Unity project. It's kept here in case a
future feature (e.g. region-based matchmaking) wants to build on it.

## Python sandbox example

```python
from opengateway_sandbox_sdk import ClientCredentials, DeviceLocation

credentials = ClientCredentials(
    client_id='your_client_id',
    client_secret='your_client_secret'
)

customer_phone_number = "+34666666666"

devicelocation_client = DeviceLocation(credentials=credentials, phone_number=customer_phone_number)
```

`client_id` / `client_secret` come from registering an application in the
[Open Gateway sandbox](https://opengateway.telefonica.com/en/discover-apis). `customer_phone_number` is the MSISDN
(in E.164 format) of the device whose location is being checked; the value above is Telefónica's documented sandbox
test number, not a real subscriber.

## Notes

- This API returns real subscriber location data in production — it requires explicit user consent and telco-level
  authorization, and should never be called against a real phone number without that consent.
- The SDK is Python-only; there is no official C#/Unity client. A Unity integration would need to call the
  underlying CAMARA REST API directly (OAuth2 client-credentials flow + a `POST` to the Device Location `verify` or
  `retrieve` endpoint).
