from dataclasses import dataclass
from typing import Optional

from opengateway_sandbox_sdk import ClientCredentials, DeviceLocation

from .config import load_client_credentials


@dataclass
class LocationVerificationResult:
    verified: bool


class DeviceLocationVerifier:
    """Verifies a customer's claimed location against their carrier network
    location via the CAMARA/Open Gateway Device Location Verification API.

    Requires server-side credentials (OPENGATEWAY_CLIENT_ID/SECRET) - never
    embed these in a client application.
    """

    def __init__(self, client_id: Optional[str] = None, client_secret: Optional[str] = None):
        if client_id is None or client_secret is None:
            client_id, client_secret = load_client_credentials()
        self._credentials = ClientCredentials(client_id=client_id, client_secret=client_secret)

    def verify(
        self,
        phone_number: str,
        latitude: float,
        longitude: float,
        accuracy_km: float,
    ) -> LocationVerificationResult:
        client = DeviceLocation(credentials=self._credentials, phone_number=phone_number)
        return LocationVerificationResult(verified=client.verify(latitude, longitude, accuracy_km))
