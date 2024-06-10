extends Node

# These can be updated from the editor
@export var BASE_URL = ""
@export var AUTHENTICATE_URL = BASE_URL + "/authenticate"
@export var VALIDATE_URL = BASE_URL + "/validate"


@onready var http_request = HTTPRequest.new()

func _ready():
	add_child(http_request)
	http_request.request_completed.connect(self._on_request_completed)

func authenticate_user(username: String, smartWalletAdress: String, signature: String):
	var data = {
		"username": username,
		"address": smartWalletAdress,
		"signature": signature
	}

	var json = JSON.new()
	var json_data = json.print(data)
	var headers = ["Content-Type: application/json"]
	http_request.request(AUTHENTICATE_URL, headers, HTTPClient.METHOD_POST, json_data)

func validate_token(username: String, session_token: String):
	var data = {
		"username": username,
		"session_token": session_token
	}
	var json = JSON.new()

	var json_data = json.parse(data)
	var headers = ["Content-Type: application/json"]
	http_request.request(VALIDATE_URL, headers, HTTPClient.METHOD_POST, json_data)

func _on_request_completed(result: int, response_code: int, headers: Array[String], body: PackedByteArray):
	if response_code == 200:
		var json = JSON.new()
		var response = json.parse(body.get_string_from_utf8())
		if response.error == OK:
			var result_data = response.result
			print(result_data)  # Process the result as needed
			if AUTHENTICATE_URL in http_request.get_requested_url():
				print("Authentication successful, session token: %s" % result_data["session_token"])
			elif VALIDATE_URL in http_request.get_requested_url():
				print("Validation result: %s" % result_data["valid"])
		else:
			print("JSON parsing error: %s" % response.error_string)
	else:
		print("HTTP request failed with response code: %d" % response_code)
