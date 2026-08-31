import os


class MissingCredentialsError(RuntimeError):
    pass


def load_client_credentials():
    client_id = os.environ.get("OPENGATEWAY_CLIENT_ID")
    client_secret = os.environ.get("OPENGATEWAY_CLIENT_SECRET")
    if not client_id or not client_secret:
        raise MissingCredentialsError(
            "Set OPENGATEWAY_CLIENT_ID and OPENGATEWAY_CLIENT_SECRET "
            "(see .env.example) before using DeviceLocationVerifier."
        )
    return client_id, client_secret
