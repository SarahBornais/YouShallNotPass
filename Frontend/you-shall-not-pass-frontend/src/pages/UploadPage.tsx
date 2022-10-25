import { useState } from "react";
import { Alert, Button, Col, Form, Modal, Row } from "react-bootstrap";
import Moment from "react-moment";
import moment from 'moment';

function UploadPage() {
    const defaultExpiration = moment();
    console.log(moment().format());
    defaultExpiration.add(1, "days");
    const [secretData, setSecretData] = useState({
        contentType: 0,
        label: "",
        expirationDate: defaultExpiration.format("yyyy-MM-DD"),
        expirationTime: defaultExpiration.format("HH:mm"),
        maxAccessCount: "",
        data: ""
    });

    const [show, setShow] = useState(false);
    const handleClose = () => setShow(false);

    const [id, setId] = useState();
    const [key, setKey] = useState();

    const uploadSecret = (e: any) => {
        e.preventDefault();
        const body = {
            contentType: 0,
            label: "",
            expirationDate: defaultExpiration.format("yyyy-MM-DD"),
            expirationTime: defaultExpiration.format("HH:mm"),
            maxAccessCount: "",
            data: ""
        };
        Object.assign(body, secretData);
        body["expirationDate"] = new Date(`${secretData.expirationDate} ${secretData.expirationTime}`).toISOString()
        body["data"] = btoa(body["data"]);
        console.log(JSON.stringify(body));
        fetch(`https://youshallnotpassbackend.azurewebsites.net/vault`, { 
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(body) 
        })
            .then((response) => response.json())
            .then((data) => {
                console.log(JSON.stringify(data))
                setId(data.id);
                setKey(data.key);
            })
            .finally(() => setShow(true));
    };

    const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        console.log(event.target.value);
        setSecretData({ ...secretData, [event.target.name]: event.target.value });
    };

    return (
        <div>
            <h1>Upload Secure Data</h1>
            <hr />
            <Form>
                <Form.Group className="mb-3">
                    <Form.Label>Description</Form.Label>
                    <Form.Control type="text" id="labelInput" placeholder="My Secret File" name="label" value={secretData.label} onChange={handleChange} />
                </Form.Group>

                <Form.Group className="mb-3" controlId="expiryDateInput">
                    <Form.Label>Expiry Date</Form.Label>
                    <Row>
                        <Col>
                            <Form.Control type="date" name="expirationDate" value={secretData.expirationDate} onChange={handleChange} />
                        </Col>
                        <Col>
                            <Form.Control type="time" name="expirationTime" value={secretData.expirationTime} onChange={handleChange}  />
                        </Col>
                    </Row>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Maximum Number of Accesses</Form.Label>
                    <Form.Control type="text" placeholder="Unlimited" name="maxAccessCount" value={secretData.maxAccessCount} onChange={handleChange} />
                </Form.Group>

                <Form.Group className="mb-3" controlId="secretInput">
                    <Form.Label>Secret</Form.Label>
                    <Form.Control as="textarea" rows={3} name="data" value={secretData.data} onChange={handleChange} />
                </Form.Group>

                <Button variant="primary" type="submit" onClick={uploadSecret}>
                    Get Secure Link
                </Button>
            </Form>

            <Modal show={show} dialogClassName="modal-90w">
                <Modal.Header closeButton>
                    <Modal.Title>Secure Link</Modal.Title>
                </Modal.Header>

                <Modal.Body>
                    <Alert variant="primary">
                        <a href={`/view?id=${id}&key=${key}`}>https://youshallnotpass.org/view?id={id}&key={key}</a>
                    </Alert> 
                </Modal.Body>

                <Modal.Footer>
                    <Button variant="primary" onClick={handleClose}>Close</Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default UploadPage;