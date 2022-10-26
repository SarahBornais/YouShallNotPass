import moment from "moment";
import { useEffect, useState } from "react";
import { Alert, Button, Card, Col, Container, Modal, Row, Spinner, Toast, ToastContainer } from "react-bootstrap";
import { useSearchParams } from "react-router-dom";

function ViewPage() {
    const [loading, setLoading] = useState(true);
    const [show, setShow] = useState(false);
    const confirmDelete = () => setShow(true);
    const handleClose = () => setShow(false);

    const [errorMessage, setErrorMesssage] = useState("Unexpected error. Please try again.");
    const [toastShow, setToastShow] = useState(false);
    const handleToastClose = () => setToastShow(false);

    const [searchParams] = useSearchParams();
    const id = searchParams.get("id");
    const key = searchParams.get("key");

    const [label, setLabel] = useState("Secret Not Found");
    const [secret, setSecret] = useState("secret not found");
    const [timesAccessed, setTimesAccessed] = useState(0);
    const [maxAccesses, setMaxAccesses] = useState("0");
    const [expiration, setExpiration] = useState("");

    useEffect(() => {
        fetch(`https://youshallnotpassbackend.azurewebsites.net/vault?id=${id}&key=${key}`)
            .then((response) => response.json())
            .then((data) => {
                setLabel(data.label);
                setSecret(atob(data.data));
                setTimesAccessed(data.timesAccessed);
                setMaxAccesses(data.maxAccessCount >= 100000 ? "Unlimited" : data.maxAccessCount);
                setExpiration(moment(new Date(data.expirationDate)).format("YYYY-MM-DD hh:MM A z"));
                setLoading(false);
            })
            .catch(() => {
                setErrorMesssage("Unexpected error fetching secret. Please try again.");
                setToastShow(true);
            })
            .finally(() => setShow(false));
    }, [id, key]);

    const handleDelete = () => {
        fetch(`https://youshallnotpassbackend.azurewebsites.net/vault?id=${id}`, { method: "DELETE" })
        .catch((error) => {
            setErrorMesssage("Unexpected error deleting secret. Please try again.");
            setToastShow(true);
        })
        .finally(() => setShow(false));
    };

    return loading ? (
        <div className="spin-container">
            <Spinner animation="border" variant="primary" />
        </div>
    ) : (
        <div>
            <Container>
                <Row className="header">
                    <Col xs={10}>
                        <h1>{label}</h1>
                    </Col>
                    <Col>
                        <Button variant="danger" onClick={confirmDelete}>Delete</Button>
                    </Col>
                </Row>
            </Container>
            <hr />
            <Container>
                <Row className="header">
                    <Col>
                        <p><strong>Times accessed:</strong> {timesAccessed}/{maxAccesses}</p>
                    </Col>
                    <Col>
                        <p><strong>Expires:</strong> {expiration}</p>
                    </Col>
                </Row>
            </Container>

            <Card>
                <Card.Body>
                    <Card.Text>
                        {secret}
                    </Card.Text>
                </Card.Body>
            </Card>

            <Modal show={show} onHide={handleClose}>
                <Modal.Header closeButton>
                    <Modal.Title>Confirm Deletion</Modal.Title>
                </Modal.Header>

                <Modal.Body>
                    <p>For your security, this action cannot be undone.</p>
                </Modal.Body>

                <Modal.Footer>
                    <Button variant="secondary" onClick={handleClose}>Close</Button>
                    <Button variant="danger" onClick={handleDelete}>Delete</Button>
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

export default ViewPage;