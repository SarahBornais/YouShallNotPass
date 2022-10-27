import './App.css';
import {
    BrowserRouter as Router,
    Routes,
    Route
} from "react-router-dom";
import UploadPage from './pages/UploadPage';
import ViewPage from './pages/ViewPage';
import { Container, Navbar } from 'react-bootstrap';

function App() {
    return (
        <div>
            <Navbar bg="dark" variant="dark">
                <Container>
                    <Navbar.Brand href="/">
                        <img
                        src="./logo.png"
                        width="30"
                        height="30"
                        className="d-inline-block align-top"
                        alt=""
                        />
                        You Shall Not Pass!
                    </Navbar.Brand>
                </Container>
            </Navbar>
            <Container>
                <Router>
                    <Routes>
                        <Route path="/" element={<UploadPage />} />
                        <Route path="/view" element={<ViewPage />} />
                    </Routes>
                </Router>
            </Container>
        </div>
    );
}

export default App;
