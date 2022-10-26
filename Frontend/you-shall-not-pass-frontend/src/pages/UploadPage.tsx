import { useState } from "react";
import { Alert, Button, Col, Form, Modal, Row, Toast, ToastContainer } from "react-bootstrap";
import moment from 'moment';
import * as Icon from 'react-bootstrap-icons';

function UploadPage() {
    const defaultExpiration = moment();
    defaultExpiration.add(1, "days");
    const [secretData, setSecretData] = useState({
        contentType: 0,
        label: "",
        expirationType: "24HOURS",
        expirationDate: defaultExpiration.format("yyyy-MM-DD"),
        expirationTime: defaultExpiration.format("HH:mm"),
        maxAccessCount: "",
        data: ""
    });

    const [customDate, setCustomDate] = useState(false);

    const [errorMessage, setErrorMesssage] = useState("Something went wrong unexpectedly. Please try again.");
    const [toastShow, setToastShow] = useState(false);
    const handleToastClose = () => setToastShow(false);

    const [show, setShow] = useState(false);
    const handleClose = () => setShow(false);

    const [id, setId] = useState();
    const [key, setKey] = useState();

    const uploadSecret = (e: any) => {
        e.preventDefault();
        const body: any = {
            "contentType": 0,
            "label": secretData.label,
            "expirationDate": moment(`${secretData.expirationDate} ${secretData.expirationTime}`, 'YYYY-MM-DD HH:mm').toDate().toISOString(),
            "data": btoa(secretData["data"])
        };
        if (secretData.maxAccessCount.length > 0) {
            body["maxAccessCount"] = secretData.maxAccessCount;
        } else {
            body["maxAccessCount"] = 100000;
        }

        fetch(`https://youshallnotpassbackend.azurewebsites.net/vault`, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
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
                setErrorMesssage("Unexpected error saving secret. Please try again.");
                setToastShow(true);
            });
    };

    const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setSecretData({ ...secretData, [event.target.name]: event.target.value });
        if (event.target.name === "expirationDate" || event.target.name === "expirationTime") {
            console.log("Setting exp type")
            secretData.expirationType = "CUSTOM";
        }
    };

    const [validated, setValidated] = useState(false);

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

    function copyLink() {
        navigator.clipboard.writeText(`https://youshallnotpass.org/view?id=${id}&key=${key}`);
        document.getElementById("copy-success")?.removeAttribute("hidden");
    }

    function onExpirySelectChange(event: any) {
        console.log("exp change");
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
            setSecretData({ ...secretData, 
                ["expirationDate"]: expiration.format("yyyy-MM-DD"), 
                ["expirationTime"]: expiration.format("HH:mm") 
            });
        }
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
                                value={secretData.expirationDate} 
                                disabled={!customDate}
                                onChange={handleChange} />
                        </Col>
                        <Col>
                            <Form.Control 
                                type="time" 
                                name="expirationTime" 
                                value={secretData.expirationTime} 
                                disabled={!customDate}
                                onChange={handleChange} />
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
                    <Form.Control required as="textarea" rows={3} name="data" value={secretData.data} onChange={handleChange} />
                    <Form.Control.Feedback type="invalid">
                        Please enter a secret
                    </Form.Control.Feedback>
                </Form.Group>

                <Button variant="primary" type="submit">
                    Get Secure Link
                </Button>
            </Form>

            <Modal show={show} dialogClassName="modal-90w" onHide={handleClose}>
                <Modal.Header closeButton>
                    <Modal.Title>Secure Link</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Alert variant="primary">
                        <a href={`/view?id=${id}&key=${key}`}>https://youshallnotpass.org/view?id={id}&key={key}</a>
                    </Alert>
                </Modal.Body>
                <Modal.Footer>
                    <p hidden id="copy-success" className="copy-success"><Icon.CheckCircleFill color="green" size={16} /> Coppied to clipboard</p>
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

export default UploadPage;