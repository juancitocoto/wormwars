from dotenv import load_dotenv

from device_location_service import DeviceLocationVerifier


def main():
    load_dotenv()
    verifier = DeviceLocationVerifier()
    result = verifier.verify(
        phone_number="+34666666666",
        latitude=40.4168,
        longitude=-3.7038,
        accuracy_km=2,
    )
    print(f"Verified: {result.verified}")


if __name__ == "__main__":
    main()
