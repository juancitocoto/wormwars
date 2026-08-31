from unittest.mock import MagicMock, patch

import pytest

from device_location_service.client import DeviceLocationVerifier
from device_location_service.config import MissingCredentialsError


@patch("device_location_service.client.DeviceLocation")
@patch("device_location_service.client.ClientCredentials")
def test_verify_true_result(mock_credentials, mock_device_location):
    mock_client = MagicMock()
    mock_client.verify.return_value = True
    mock_device_location.return_value = mock_client

    verifier = DeviceLocationVerifier(client_id="id", client_secret="secret")
    result = verifier.verify(phone_number="+34666666666", latitude=1.0, longitude=2.0, accuracy_km=5)

    assert result.verified is True
    mock_device_location.assert_called_once_with(
        credentials=mock_credentials.return_value, phone_number="+34666666666"
    )
    mock_client.verify.assert_called_once_with(1.0, 2.0, 5)


@patch("device_location_service.client.DeviceLocation")
@patch("device_location_service.client.ClientCredentials")
def test_verify_false_result(mock_credentials, mock_device_location):
    mock_client = MagicMock()
    mock_client.verify.return_value = False
    mock_device_location.return_value = mock_client

    verifier = DeviceLocationVerifier(client_id="id", client_secret="secret")
    result = verifier.verify(phone_number="+34666666666", latitude=1.0, longitude=2.0, accuracy_km=5)

    assert result.verified is False


def test_missing_credentials_raises(monkeypatch):
    monkeypatch.delenv("OPENGATEWAY_CLIENT_ID", raising=False)
    monkeypatch.delenv("OPENGATEWAY_CLIENT_SECRET", raising=False)

    with pytest.raises(MissingCredentialsError):
        DeviceLocationVerifier()
