import { useParams } from 'react-router-dom'
import { PalestraForm } from '../shared/PalestraForm'
export function AtualizarPalestraPage() { const { id } = useParams(); return <PalestraForm palestraId={id} /> }
