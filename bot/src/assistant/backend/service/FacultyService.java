package assistant.backend.service;
import assistant.backend.BackendClient; import assistant.backend.dto.*; import com.fasterxml.jackson.databind.JsonNode; import java.util.*; import java.util.stream.StreamSupport;
public final class FacultyService {
 public long getRecordCount(String department){return faculty(0,100,department).size();} public List<EmailDTO> getFacultyEmails(){return List.of();}
 public List<FacultyDTO> getFaculty(int page,int size,String department){return faculty(page,size,department);} public Optional<FacultyDTO> getProfessor(EmailDTO email){return Optional.empty();} public int addProfessor(FacultyDTO p){return 0;}
 private List<FacultyDTO> faculty(int page,int size,String search){String p="/api/v1/inelicom/faculties?page_index="+page+"&limit="+size; return BackendClient.getData(p).map(n->n).stream().flatMap(n->StreamSupport.stream(n.spliterator(),false)).map(this::map).toList();}
 private FacultyDTO map(JsonNode n){FacultyDTO d=new FacultyDTO();d.setId(n.path("faculty_id").asInt());d.setName(n.path("name").asText());d.setDescription("");d.setJobentitlement("Faculty");d.setOffice("");d.setContact(new ContactDTO());return d;}
}
