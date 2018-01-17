using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IaListener : MonoBehaviour {
	private int locationCount;
	private Vector2 latlon0;
	private IndoorAtlas.WGSConversion converter;
	private float weightSum;

	void Start () {
		locationCount = 0;
		latlon0.x = 0.0f;
		latlon0.y = 0.0f;
		converter = new IndoorAtlas.WGSConversion ();
		weightSum = 0.0f;
	}
	
	void Update () {
	}

	void Awake() {
	}

	void onLocationChanged(string data) {
		IndoorAtlas.Location location = JsonUtility.FromJson<IndoorAtlas.Location>(data);

		// Use the first locations to set origin. Weight the received locations with the received accuracy
		// (the most accurate locations will become more important).
		if (locationCount < 10) {
			float weight = Mathf.Pow(1.0f / (float)(location.accuracy), 2.0f);
			latlon0 += weight * (new Vector2 ((float)location.latitude, (float)location.longitude));
			weightSum += weight;
			locationCount++;
		} else if (converter != null) {
			// Set the origin and convert location to (a scaled) East-North position.
			// With a physically meaningful "East-North" aligned 3D environment the origin could be
			// an arbitrarily chosen point whose WGS coordinates can be determined accurately.
			converter.setOrigin (
				(double)(latlon0.x / weightSum),
				(double)(latlon0.y / weightSum)
			);
			Vector2 EN = 0.5f * converter.WGStoEN(
				location.latitude,
				location.longitude
			);

			// E.g. floor information could be used to tune the altitude.
			Camera.main.transform.position = new Vector3(EN.x, 1.0f, -10.0f + EN.y);
			Debug.Log ("onLocationChanged. Metric pos: " + EN.x + ", " + EN.y);
		}
		Debug.Log ("onLocationChanged. Latlon: " + location.latitude + ", " + location.longitude);
	}

	void onStatusChanged(string data) {
		IndoorAtlas.Status serviceStatus = JsonUtility.FromJson<IndoorAtlas.Status> (data);
		Debug.Log ("onStatusChanged " + serviceStatus.status);
	}

	void onHeadingChanged(string data) {
		IndoorAtlas.Heading heading = JsonUtility.FromJson<IndoorAtlas.Heading>(data);
		Debug.Log ("onHeadingChanged " + heading.heading);
	}

	void onOrientationChange(string data) {
		Quaternion orientation = JsonUtility.FromJson<IndoorAtlas.Orientation>(data).getQuaternion();
		Quaternion rot = Quaternion.Inverse(new Quaternion(orientation.x, orientation.y, -orientation.z, orientation.w));
		Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)) * rot;
	}

	void onEnterRegion (string data) {
		IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
		Debug.Log ("onEnterRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
	}

	void onExitRegion (string data) {
		IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
		Debug.Log ("onExitRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
	}
}
