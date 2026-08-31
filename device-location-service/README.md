# device-location-service

Server-side Python wrapper around the [CAMARA/Open Gateway Device Location
Verification API](https://developers.opengateway.telefonica.com/docs/devicelocation)
(via the `opengateway-sandbox-sdk` package). Given a customer's phone number
and a claimed latitude/longitude, it asks the carrier network whether the
device is actually within that circle - a signal that's useful alongside (or
instead of) IP geolocation for a security guardrail, since it's based on
network location rather than a spoofable client-reported IP.

This is a standalone backend component, separate from the `wormwars` Unity
game in the rest of this repo.

## Setup

```bash
cd device-location-service
python -m venv .venv && source .venv/bin/activate
pip install -r requirements-dev.txt
cp .env.example .env  # then fill in your sandbox credentials
```

Get sandbox `client_id`/`client_secret` from the
[Open Gateway sandbox](https://developers.opengateway.telefonica.com/docs/usethesandbox).
**Never commit `.env` or hardcode credentials** - this service must run
server-side; the client secret must not reach any game client or browser.

## Usage

```python
from device_location_service import DeviceLocationVerifier

verifier = DeviceLocationVerifier()  # reads OPENGATEWAY_CLIENT_ID/SECRET from env
result = verifier.verify(
    phone_number="+34666666666",
    latitude=40.4168,
    longitude=-3.7038,
    accuracy_km=2,
)
print(result.verified)  # bool
```

See `examples/verify_example.py` for a runnable version.

## Testing

```bash
pytest
```

Tests mock the SDK, so they run without real sandbox credentials.

## Notes

`opengateway-sandbox-sdk`'s `DeviceLocation.verify(latitude, longitude, accuracy, phone_number=None)`
returns a plain `bool` and has no `max_age`/freshness parameter in the
installed version (`inspect.signature` was checked directly against the
package, since the online SDK reference was unreachable from this
environment) - so this wrapper doesn't expose one either. If a newer SDK
version adds one, extend `DeviceLocationVerifier.verify()` accordingly.
