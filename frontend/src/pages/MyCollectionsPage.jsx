import Header from '../components/Header';
import './MyCollectionsPage.scss';

export default function MyCollectionsPage({currentUser}){

    return(
        <div className='container'>
            <Header currentUser={currentUser}/>
            <main className='content-area'><h1>My Collections Page</h1></main>
        </div>
    );
}