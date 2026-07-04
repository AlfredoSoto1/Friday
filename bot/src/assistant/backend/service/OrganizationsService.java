package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.OrganizationDTO;
import com.fasterxml.jackson.databind.JsonNode;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

public final class OrganizationsService {
  public List<String> getOrganizationNames(int page, int size) { return organizations(page,size,null).stream().map(OrganizationDTO::getName).toList(); }
  public List<OrganizationDTO> getAllOrganizations(int page, int size) { return organizations(page,size,null); }
  public Optional<OrganizationDTO> getOrganization(String name) { return organizations(0,25,name).stream().filter(o -> o.getName().equalsIgnoreCase(name)).findFirst(); }
  private List<OrganizationDTO> organizations(int page,int size,String search) {
    String path="/api/v1/inelicom/organizations?page_index="+page+"&limit="+size;
    if(search!=null) path+="&search="+URLEncoder.encode(search,StandardCharsets.UTF_8);
    return BackendClient.getData(path).map(d->d).stream().flatMap(n->StreamSupport.stream(n.spliterator(),false)).map(this::map).toList();
  }
  private OrganizationDTO map(JsonNode n){ OrganizationDTO d=new OrganizationDTO(); d.setId(n.path("organization_id").asInt()); d.setName(n.path("name").asText()); d.setDescription(n.path("description").asText()); return d; }
}
