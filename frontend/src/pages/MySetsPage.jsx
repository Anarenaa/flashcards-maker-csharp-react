import Header from '../components/Header';
import './MySetsPage.scss';

export default function MySetsPage({currentUser}){

    return(
        <div className='container'>
            <Header currentUser={currentUser}/>
            <main className='content-area'><h1>My Sets Page</h1></main>
        </div>
    );
}