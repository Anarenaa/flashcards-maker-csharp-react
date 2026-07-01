import Header from '../components/Header';
import './MyProfilePage.scss';

export default function MyProfilePage({currentUser}){

    return(
        <div className='container'>
            <Header currentUser={currentUser}/>
            <main className='content-area'><h1>My Profile Page</h1></main>
        </div>
    );
}