import Header from '../components/Header';
import './SettingsPage.scss';

export default function SettingsPage({currentUser}){

    return(
        <div className='container'>
            <Header currentUser={currentUser}/>
            <main className='content-area'><h1>Settings Page</h1></main>
        </div>
    );
}