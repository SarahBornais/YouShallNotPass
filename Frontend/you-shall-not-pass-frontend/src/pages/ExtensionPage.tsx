import { useState } from "react";
import { Alert, Button, Col, Form, Modal, Row, Toast, ToastContainer } from "react-bootstrap";
import moment from 'moment';
import * as Icon from 'react-bootstrap-icons';
import ReCAPTCHA from "react-google-recaptcha";

function ExtensionPage() {
    const PDF_CONTENT = 0;
    const IMAGE_CONTENT = 1;
    const TEXT_CONTENT = 2;
    const BASE_URL = "https://youshallnotpassbackend.azurewebsites.net";
    const API_KEY = "54a72ec5a0a29aa7b9d800d084c322dfd06bd7a645c6357b4a532603";
    const SERVICE_NAME = "frontend";
    const CAPTCHA_KEY = "6Ldorv0iAAAAAPxWtrETU8WsSxhxbFl3ssxWGi51";

    const defaultExpiration = moment();
    defaultExpiration.add(1, "days");
    const [secretData, setSecretData] = useState({
        contentType: 0,
        label: "",
        expirationType: "24HOURS",
        expirationDate: defaultExpiration.format("yyyy-MM-DD"),
        expirationTime: defaultExpiration.format("HH:mm"),
        maxAccessCount: "",
        data: "",
        fileData: "",
        securityQuestion: "",
        securityAnswer: ""
    });

    const [captchaToken, setCaptchaToken] = useState("");

    const [customDate, setCustomDate] = useState(false);
    const [dateMin, setDateMin] = useState(moment().format("yyyy-MM-DD"));
    const [timeMin, setTimeMin] = useState(moment().format("HH:mm"));

    const [secretManualEntry, setSecretManualEntry] = useState(false);

    const [errorMessage, setErrorMessage] = useState("Something went wrong unexpectedly. Please try again.");
    const [toastShow, setToastShow] = useState(false);
    const handleToastClose = () => setToastShow(false);

    const [show, setShow] = useState(false);
    const handleClose = () => setShow(false);

    const [id, setId] = useState();
    const [key, setKey] = useState();

    const [validated, setValidated] = useState(false);

    const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setSecretData({ ...secretData, [event.target.name]: event.target.value });
        if (event.target.name === "expirationDate") {
            if (moment(event.target.value).isSameOrBefore(moment())) {
                setTimeMin(moment().format("HH:mm"));
            } else {
                setTimeMin("00:00");
            }
        }
    };

    const onExpirySelectChange = (event: any) => {
        secretData.expirationType = event.target.value;
        if (event.target.value === "CUSTOM") {
            setCustomDate(true);
        } else {
            setCustomDate(false);
            const expiration = moment();
            switch (event.target.value) {
                case "1HOUR":
                    expiration.add(1, "hours");
                    break;
                case "24HOURS":
                    expiration.add(1, "days");
                    break;
                case "48HOURS":
                    expiration.add(2, "days");
                    break;
                case "WEEK":
                    expiration.add(1, "weeks");
                    break;
                case "MONTH":
                    expiration.add(1, "months");
                    break;
            }
            setSecretData({
                ...secretData,
                ["expirationDate"]: expiration.format("yyyy-MM-DD"),
                ["expirationTime"]: expiration.format("HH:mm")
            });
        }
    }

    const onFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file: File = event.target!.files![0];
        const reader = new FileReader();
        reader.onloadend = () => {
            const fileType = (reader.result as string).split(":")[1].split(";")[0];
            setSecretData({
                ...secretData,
                ["fileData"]: (reader.result as string).split("base64,")[1],
                ["contentType"]: fileType.includes("image") ?
                    IMAGE_CONTENT : fileType.includes("pdf") ? PDF_CONTENT : TEXT_CONTENT
            });
        };
        reader.readAsDataURL(file);
    }

    const toggleSecretManualEntry = () => {
        setSecretManualEntry(!secretManualEntry);
        setSecretData({...secretData, ["contentType"]: TEXT_CONTENT});
    }

    const handleSubmit = (event: any) => {
        setValidated(true);
        const form = event.currentTarget;
        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            uploadSecret(event);
        }
    };

    const onCaptchaChange = (token: string | null) => {
        setCaptchaToken(token ? token : "");
    }

    const uploadSecret = (e: any) => {
        e.preventDefault();
        const body: any = {
            "contentType": secretData.contentType,
            "label": secretData.label,
            "expirationDate": moment(`${secretData.expirationDate} ${secretData.expirationTime}`, 'YYYY-MM-DD HH:mm').toDate().toISOString(),
            "data": secretData.fileData.length > 0 ? secretData.fileData : btoa(secretData["data"])
        };
        if (secretData.maxAccessCount.length > 0) {
            body["maxAccessCount"] = secretData.maxAccessCount;
        } else {
            body["maxAccessCount"] = 100000;
        }

        fetch(`${BASE_URL}/security/authenticate?ServiceName=${SERVICE_NAME}&SecretKey=${API_KEY}`)
            .then((response) => response.json())
            .then((authData) => {
                fetch(`${BASE_URL}/vault`, {
                    method: "POST",
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${authData.token}`,
                        'CaptchaToken': captchaToken
                    },
                    body: JSON.stringify(body)
                })
                    .then((response) => response.json())
                    .then((data) => {
                        setId(data.id);
                        setKey(data.key);
                        setShow(true);
                    })
                    .catch(() => {
                        setErrorMessage("Unexpected error saving secret. Please try again.");
                        setToastShow(true);
                    });
            })


    };

    const copyLink = () => {
        navigator.clipboard.writeText(`URL to view secret: https://youshallnotpass.org/view?id=${id}\n\nSecret key: ${key}`);
        document.getElementById("copy-success")?.removeAttribute("hidden");
    }

    return (
        <div>
            <h1>Upload Secure Data</h1>
            <hr />
            <Form noValidate validated={validated} onSubmit={handleSubmit}>
                <Form.Group className="mb-3">
                    <Form.Label>Description</Form.Label>
                    <Form.Control
                        required
                        type="text"
                        id="labelInput"
                        placeholder="My Secret File"
                        name="label"
                        value={secretData.label}
                        onChange={handleChange} />
                    <Form.Control.Feedback type="invalid">
                        Please enter a description
                    </Form.Control.Feedback>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Expiration</Form.Label>
                    <Row>
                        <Col>
                            <Form.Select value={secretData.expirationType} onChange={onExpirySelectChange}>
                                <option value="1HOUR">1 Hour</option>
                                <option value="24HOURS">24 Hours</option>
                                <option value="48HOURS">48 Hours</option>
                                <option value="WEEK">1 Week</option>
                                <option value="MONTH">1 Month</option>
                                <option value="CUSTOM">Custom</option>
                            </Form.Select>
                        </Col>
                        <Col>
                            <Form.Control
                                id="expiryDateInput"
                                type="date"
                                name="expirationDate"
                                min={dateMin}
                                value={secretData.expirationDate}
                                disabled={!customDate}
                                onChange={handleChange} />
                        </Col>
                        <Col>
                            <Form.Control
                                type="time"
                                name="expirationTime"
                                min={timeMin}
                                value={secretData.expirationTime}
                                disabled={!customDate}
                                onChange={handleChange} />
                            <Form.Control.Feedback type="invalid">
                                Ensure time is in the future
                            </Form.Control.Feedback>
                        </Col>
                    </Row>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Maximum Number of Accesses</Form.Label>
                    <Form.Control
                        type="number"
                        placeholder="Unlimited"
                        name="maxAccessCount"
                        value={secretData.maxAccessCount}
                        onChange={handleChange} />
                </Form.Group>

                <Form.Group className="mb-3" controlId="secretInput">
                    <Form.Label>Secret</Form.Label>
                    <Form.Control required
                                  type="file"
                                  accept=".png,.pdf"
                                  onChange={onFileUpload}
                                  hidden={secretManualEntry}
                                  disabled={secretManualEntry} />
                    <Form.Control required
                                  as="textarea" rows={3}
                                  name="data"
                                  value={secretData.data}
                                  onChange={handleChange}
                                  disabled={!secretManualEntry}
                                  hidden={!secretManualEntry} />
                    <Form.Control.Feedback type="invalid">
                        Please enter a secret
                    </Form.Control.Feedback>
                    <Form.Check
                        type="switch"
                        id="custom-switch"
                        label="Enter secret text manually"
                        checked={secretManualEntry}
                        onChange={toggleSecretManualEntry}
                    />
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Security Question</Form.Label>
                    <Form.Control
                        type="text"
                        id="labelInput"
                        name="label"
                        value={secretData.securityQuestion}
                        onChange={handleChange} />
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Security Answer</Form.Label>
                    <Form.Control
                        type="text"
                        id="labelInput"
                        name="label"
                        value={secretData.securityAnswer}
                        onChange={handleChange} />
                </Form.Group>

                <ReCAPTCHA
                    sitekey={CAPTCHA_KEY}
                    onChange={onCaptchaChange}
                />
                <br />

                <Button variant="primary" type="submit" style={{marginBottom: "50px"}}>
                    Get Secure Link
                </Button>
            </Form>

            <Modal show={show} dialogClassName="modal-90w" onHide={handleClose}>
                <Modal.Header closeButton>
                    <Modal.Title>Secure Link</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Alert variant="primary">
                        <p><strong>URL to view secret:</strong> <a href={`/view?id=${id}&key=${key}`}>https://youshallnotpass.org/view?id={id}</a></p>
                        <p><strong>Secret key:</strong> {key}</p>
                    </Alert>
                </Modal.Body>
                <Modal.Footer>
                    <p hidden id="copy-success" className="copy-success"><Icon.CheckCircleFill color="green" size={16} /> Copied to clipboard</p>
                    <Button variant="default" onClick={handleClose}>Close</Button>
                    <Button variant="primary" onClick={copyLink}>Copy</Button>
                </Modal.Footer>
            </Modal>

            <ToastContainer className="p-3" position="top-center">
                <Toast show={toastShow}>
                    <Alert variant="danger" dismissible onClose={handleToastClose}>
                        <Alert.Heading>Error</Alert.Heading>
                        <p className="error-message">{errorMessage}</p>
                    </Alert>
                </Toast>
            </ToastContainer>
        </div>
    )
}

export default ExtensionPage;