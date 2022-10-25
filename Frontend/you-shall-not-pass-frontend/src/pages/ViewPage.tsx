import { useCallback, useEffect, useState } from "react";
import { Button, Card, Col, Container, Modal, Row } from "react-bootstrap";
import { useParams, useSearchParams } from "react-router-dom";

interface SecretResponse {
    label: string,
    data: string,
    timesAccessed: number,
    maxAccessCount: number,
    expirationDate: Date
}

function ViewPage() {
    const [show, setShow] = useState(false);
    const confirmDelete = () => setShow(true);
    const handleClose = () => setShow(false);

    const [searchParams] = useSearchParams();
    const id = searchParams.get("id");
    const key = searchParams.get("key");

    //let { id, key } = useParams();

    //const id = "3f5ca3f9-7390-4883-b16f-ea22e3383227";
    //const key = "B5E93D22E68C99C9E28ACA8A570307A5";
    const url = "https://youshallnotpassbackend.azurewebsites.net/vault";

    const [label, setLabel] = useState("Secret Not Found");
    const [secret, setSecret] = useState("secret not found");
    const [timesAccessed, setTimesAccessed] = useState(0);
    const [maxAccesses, setMaxAccesses] = useState(0);
    const [expiration, setExpiration] = useState("");

    useEffect(() => {
        fetch(`https://youshallnotpassbackend.azurewebsites.net/vault?id=${id}&key=${key}`)
        .then((response) => response.json())
        .then((data) => {
            console.log(JSON.stringify(data));
            setLabel(data.label);
            setSecret(atob(data.data));
            setTimesAccessed(data.timesAccessed);
            setMaxAccesses(data.maxAccessCount);
            setExpiration(new Date(data.expirationDate).toLocaleString());
        });
    }, []);

    return (
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
            
            <Modal show={show}>
                <Modal.Header closeButton>
                    <Modal.Title>Confirm Deletion</Modal.Title>
                </Modal.Header>

                <Modal.Body>
                    <p>For your security, this action cannot be undone.</p>
                </Modal.Body>

                <Modal.Footer>
                    <Button variant="secondary" onClick={handleClose}>Close</Button>
                    <Button variant="danger" onClick={handleClose}>Delete</Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default ViewPage;